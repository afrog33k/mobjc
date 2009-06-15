#!/bin/bash
# This script takes two arguments: a version number template (eg 0.3.xxx.0)
# and the path to a file containing a build number.
# 
# The template should look something like "0.3.xxx.0" where the digits can
# be arbitrary numbers, but the xxx must appear as witten.
#
# The build number file should contain a single integer. If the file does not
# exist it is created and will contain "0". If it does exist the integer is 
# incremented, saved to the file, and the incremented value is used.
#
# The script returns the result of replacing the xxx in the template with the
# build number.
#
# Usage: ./get_version.sh 0.3.xxx.0 build_num
BASE_VERSION="$1"
FILE="$2"

if [ -f "$FILE" ]
then
	BUILD_NUM=`cat "$FILE"`
	((BUILD_NUM = $BUILD_NUM + 1))
else
	BUILD_NUM=0
fi

echo $BUILD_NUM > "$FILE"
echo $BASE_VERSION | sed "s/xxx/$BUILD_NUM/"
