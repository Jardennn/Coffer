import hashlib
import os
import shutil
from pathlib import Path
from unittest import result
import json

filters = Path.cwd() / "filters.json"

def loaddata():
    if not filters.exists():
        # I'm adding this little amount of filters just for the sake of testing, the C# implementation will have more filters than what is present here.
        data = { 
                "exclude_extensions": [],
                "max_size_mb": None,
                "exclude_folders": [],
                "skip_hidden": False
                }
        with open(filters, "w") as f:
            json.dump(data, f, indent=2)
        return data

    else:
        with open(filters, "r") as f:
            return json.load(f)
        

loadedfilters = loaddata()

sources = []

#sources = [Path("/home/jarden/Desktop/tests")]  # Hard coded for testing

sources = [Path("/home/jarden/Desktop/tests")]

def selectsources():
    while True:
        user_input = input("Enter the root paths of your sources (Enter 0 to stop) \n")

        if user_input == "0":
            print("Stop value entered, breaking loop.")
            break

        source = Path(user_input)

        sources.append(source)


def selectdestination():
    destination = Path(input("Enter the absolute path of your destination"))

    return destination


# destination = selectdestination()

# ===== PRE FLIGHT ====


def check_source_existance(sources):
    for source in sources:
        if not source.exists():
            return ("error", f"{source} was not found.")


def check_source_access(sources):
    for source in sources:
        if not os.access(source, os.R_OK):
            return ("error", f"No read permissions for {source}")


def check_destination_existance(destination):
    if not destination.exists():
        return ("error", "Destination path not found.")
    return None


def check_free_space(sources, destination, buffer_gb=2):
    needed = sum(f.stat().st_size for f in sources)
    available = shutil.disk_usage(destination).free
    if needed + (buffer_gb + 1024**3) > available:
        return (
            "error",
            f"Not enough space, {needed}GB required while only {available}GB is available.",
        )

    return None


def check_dest_write(destination):
    test_file = destination / ".backupmanagertest"
    test_file.touch()

    try:
        with open(test_file, "w") as f:
            f.write("test")
        test_file.unlink()
    except PermissionError:
        return ("error", "Destination is not writable.")

    return None


def check_same_drive(sources, destination):
    dest_device = os.stat(destination).st_dev
    for source in sources:
        if os.stat(source).st_dev == dest_device:
            return ("warn", "Source and destination are on the same physical drive.")


def check_no_circular(sources, destination):
    for source in sources:
        if destination.is_relative_to(source):
            return ("error", "Destination is inside a source folder.")
    return None


def check_file_count(sources, warn_threshold=50000):
    count = len(sources)
    if count > warn_threshold:
        return ("warn", f"{count} files queued. This may take a while.")
    return None


def check_long_paths(sources, destination):
    for source in sources:
        # full_dest = os.path.join(destination, source.relative_path)
        full_dest = destination / source.name
        if len(str(full_dest)) > 255:
            return ("warn", f"Path too long, may fail on Windows: {full_dest}")
    return None


def run_preflight(sources, destination):
    errors = []
    warns = []

    checks = [
        check_destination_existance(destination),
        check_free_space(sources, destination),
        check_dest_write(destination),
        check_same_drive(sources, destination),
        check_no_circular(sources, destination),
        check_file_count(sources, warn_threshold=50000),
        check_long_paths(sources, destination),
    ]

    for check in checks:
        if check is None:
            continue

        kind, message = check

        if kind == "error":
            errors.append(message)
        elif kind == "warn":
            warns.append(message)

    return errors, warns


selectsources()

destination = selectdestination()

errors, warns = run_preflight(sources, destination)

if warns:
    for w in warns:
        print(f"[WARN] {w}")

    confirm = input("Continue anyway? (y/n): ")

    if confirm.lower() != "y":
        print("Backup cancelled.")
        exit()

if errors:
    for e in errors:
        print(f"[ERROR] {e}")
    print("Backup cannot proceed. Fix the above errors and try again.")
    exit(1)

print("Pre-flight passed. Starting backup...")

def should_include(fpath, filters): 
    if fpath.suffix in filters["exclude_extensions"]: # Checking if the extension of the current working path is included in the filters
        return False
    
    if filters["max_size_mb"] != None: # If the filter is set to None, there will be no size limit for the transfer.
        if fpath.stat().st_size > filters["max_size_mb"] * 1024 * 1024: # Comparing both sizes in bytes 
            return False

    if any(part in filters["exclude_folders"] for part in fpath.parts): # Checking if the excluded folders are in the target path
        return False

    if filters["skip_hidden"] and fpath.name.startswith("."): # Dedicated for linux file systems for testing purposes, final product will have cross platform support.
        return False

    return True

def copying(source, destination, chunk_size_mb=4, progress_callback=None):
    chunk_size = chunk_size_mb * 1024 * 1024

    total_size = source.stat().st_size
    bytescopied = 0

    hasher = hashlib.sha256()

    with open(source, "rb") as src, open(destination, "wb") as dst:
        while True:
            chunk = src.read(chunk_size)
            if not chunk:
                break
            dst.write(chunk)
            bytescopied += len(chunk)

            hasher.update(chunk)

            if progress_callback:
                progress_callback(bytescopied, total_size)

    return hasher.hexdigest()


def progress_bar(bytescopied, total_size):
    precent = (bytescopied / total_size) * 100
    print(
        f"\r {precent:.1f}% {bytescopied // 1024**2}MB / {total_size // 1024**2}MB",
        end="",
    )

    copying(sources, destination, progress_callback=progress_bar)


def remove_nested_sources(sources):
    fullpaths = [Path(p).resolve() for p in sources]
    fullpaths.sort(key=lambda p: len(p.parts))

    filtered = []
    for path in fullpaths:
        if not any(path.is_relative_to(kept) for kept in filtered):
            filtered.append(path)

    return filtered


def get_dest_path(source, source_root, destination):
    relative = source.relative_to(
        source_root
    )  # Strips out the relative to source part so it would be 'file.ext' or 'directory/file.ext' instead of the full source path

    dest_path = (
        destination / source_root.name / relative
    )  # Connects the destination, the name of the directory of the root source and the relative

    return dest_path


def verify(source_hash, destination, chunk_size_mb=4):
    chunk_size = chunk_size_mb * 1024 * 1024
    hasher = hashlib.sha256()

    with open(destination, "rb") as d:
        while True:
            chunk = d.read(chunk_size)
            if not chunk:
                break
            hasher.update(chunk)

    dst_hash = hasher.hexdigest()
    return source_hash == dst_hash


def run_backup(sources, destination):

    for orig in sources:
        print(f"true original: {orig}")

    root_sources = remove_nested_sources(sources)  # Root of sources

    for origroot in root_sources:
        print(f"root original: {origroot}")

    for source in root_sources:  # One source out of root_sources
        for fpath in source.rglob(
            "*"
        ):  # Gets an absolute path of the file from searching for all enteries on the source root.
            print(f"source: {source}")

            if should_include(fpath, loadedfilters):

                if not fpath.is_file():
                    continue
            
                dest_path = get_dest_path(fpath, source, destination)

                print(f"destination: {dest_path}")

                dest_path.parent.mkdir(parents=True, exist_ok=True)

                print(f"\n{fpath.name}", end="  ....  ")

                src_hash = copying(fpath, dest_path, progress_callback=progress_bar)
                verifying = verify(src_hash, dest_path)

                if verifying:
                    print("OK")
                else:
                    print("FAILED - checksum mismatch")

        print("\nFile transfer done.")


run_backup(sources, destination)
