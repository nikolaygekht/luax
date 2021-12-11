<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html" encoding="utf-8"/>
    <xsl:template match="/" >
<html>
 <head>
    <title>Coverage report</title>
   <style>
    a, p, td, th, ul, ol, li { font-family: Verdana, Trebuchet MS, Arial; font-size: 12pt;  }
    h1 { font-family: Verdana, Trebuchet MS, Arial; font-size: 16pt;  }
    h2 { font-family: Verdana, Trebuchet MS, Arial; font-size: 14pt;  }
    pre { background-color: white; font-size: 11pt; }
    th, td { text-align: left; }
    th { background-color: #f1f1f1; }
    th.right, td.right { text-align: right; }
    pre.nostatement { color: gray; }
    pre.covered { color: green; }
    pre.notcovered { color: red; }
   </style>
 </head>

 <body>
    <h1>Overall Stats</h1>
    <table width="100%">
        <tr><th>Metrics</th><th class="right">Value</th></tr>
        <tr><td>Total statements</td><td  class="right"><xsl:value-of select="/report/@total-statements"/></td></tr>
        <tr><td>Executed statements</td><td  class="right"><xsl:value-of select="/report/@covered-statements"/></td></tr>
        <tr><td>Coverage</td><td  class="right"><xsl:value-of select="/report/@coverage"/></td></tr>
    </table>
    <h1>Class Stats</h1>
    <table width="100%">
        <tr><th colspan="2">Class/Method</th><th  class="right">Coverage</th></tr>
        <xsl:for-each select="/report/class">
            <xsl:sort select="@name" />
            <tr><td colspan="2"><b><pre><xsl:value-of select="./@name" /></pre></b></td>
                <td  class="right"><b><xsl:value-of select="./@coverage"/></b></td></tr>
            <xsl:for-each select="./method">
                <xsl:sort select="@name" />
            <tr><td>&#160;</td><td><pre><xsl:value-of select="../@name" />.<xsl:value-of select="./@name" />()</pre></td>
                <td  class="right"><xsl:value-of select="./@coverage"/></td></tr>
            </xsl:for-each>
        </xsl:for-each>
    </table>
    <h1>Files</h1>
    <xsl:variable name="root" select="/" />
    <xsl:for-each select="distinct-values(/report/class/@location-file)">
        <xsl:sort select="." />
        <xsl:variable name="file" select="." />
        <xsl:variable name="content" select="tokenize(unparsed-text(.), '\n')" />
        <h2><xsl:value-of select="$file" /></h2>
        <table width="100%">
        <xsl:for-each select="$content">
            <xsl:variable name="line" select="position()" />
            <xsl:variable name="reportline" select="$root/report/class[@location-file eq $file]/method/statement[@location-line eq string($line)]" />
            <xsl:variable name="linestyle" >
            <xsl:choose>
                <xsl:when test="count($reportline) eq 0">nostatement</xsl:when>
                <xsl:when test="$reportline/@count eq '0'">notcovered</xsl:when>
                <xsl:otherwise>covered</xsl:otherwise>
            </xsl:choose>
            </xsl:variable>
            <tr>
                <td><xsl:value-of select="$line"/></td>
                <td><pre><xsl:attribute name="class" select="$linestyle" /><xsl:value-of select="."/></pre></td>
            </tr>
        </xsl:for-each>
        </table>
    </xsl:for-each>
 </body>
</html>
    </xsl:template>
</xsl:stylesheet>