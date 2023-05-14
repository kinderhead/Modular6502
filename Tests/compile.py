import sys
import os

base = sys.argv[1].split(".")[0]
os.system(f"ca65 {sys.argv[1]} -o {base}.o")
os.system(f"ld65 -C test.cfg -o {base}.rom {base}.o")
