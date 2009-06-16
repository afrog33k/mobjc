#!/bin/bash
# Used to create AssemblyVersion.cs files.
# Note that this script is installed into /usr/local/bin by the install make target.
#
# Usage: mgen_version.sh 1.3.57.0 source/AssemblyVersion.cs
VERSION="$1"
FILE="$2"

echo "// Machine generated: do not manually edit." > ${FILE}
echo "using System.Reflection;" >> ${FILE}
echo "" >> ${FILE}
echo "[assembly: AssemblyVersion(\"${VERSION}\")]" >> ${FILE}
touch -t 0012221500 ${FILE}		# don't regenerate assemblies if only the build number changed
