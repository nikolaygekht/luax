cd ../LuaX.Test
./cover.sh
cd ../CoverageReport
saxonb-xslt -s:../LuaX.Test/coverage.xml -xsl:coverage-report.xsl -o:coverage.html