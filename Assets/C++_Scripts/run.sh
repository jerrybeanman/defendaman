
#!/bin/sh

export LD_LIBRARY_PATH=".:$LD_LIBRARY_PATH"

./test_build.x86_64 "$@"
