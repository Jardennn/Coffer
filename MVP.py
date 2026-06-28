import os
import shutil
import warnings
from pathlib import Path
from unittest import result

# profile_file = Path.cwd() / "BackupManagerConf" / "profile.json"
# lastrun_file = Path.cwd() / "BackupManagerConf" / "lastrun.json"

# TODO: should eventually add config and lastrun jsons eventually. Should work better with what claude wrote and with the adaptation of funtions.
# The program would ask the user for if they would want to save their current run as a profile for future use and it will save every last run in the last run file.

sources = []


def selectsources():
    while True:
        user_input = input(
            "Enter the absolute path of your source (Enter 0 to stop) \n"
        )

        if user_input == "0":
            print("Stop value entered, breaking loop.")
            break

        source = Path(user_input)

        sources.append(source)


def selectdestination():
    destination = Path(input("Enter the absolute path of your destination"))

    return destination


# destination = selectdestination()

# ===== PRE FLIGHT SHIT ====


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
