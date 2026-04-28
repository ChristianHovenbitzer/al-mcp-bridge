<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="text" />
  
  <xsl:template match="para">
    <xsl:apply-templates />
    <xsl:text>&#10;</xsl:text>
  </xsl:template>
  
  <xsl:template match="b">
    <xsl:text>**</xsl:text>
      <xsl:apply-templates />
    <xsl:text>**</xsl:text>
  </xsl:template>

  <xsl:template match="i">
    <xsl:text>*</xsl:text>
      <xsl:apply-templates />
    <xsl:text>*</xsl:text>
  </xsl:template>
  
  <xsl:template match="c">
    <xsl:text>`</xsl:text>
    <xsl:apply-templates />
    <xsl:text>`</xsl:text>
  </xsl:template>

  <xsl:template match="code">
    <xsl:text>``` AL</xsl:text>
    <xsl:apply-templates />
    <xsl:text>```</xsl:text>
  </xsl:template>

  <xsl:template match="paramref">
    <xsl:if test="normalize-space(@name)">
      <xsl:text>`</xsl:text>
        <xsl:value-of select="@name" />
      <xsl:text>`</xsl:text>
    </xsl:if>
  </xsl:template>

  <xsl:template match="summary">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="example">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="remarks">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="param">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="returns">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="value">
    <xsl:apply-templates />
  </xsl:template>
  

  <xsl:template match="list">
    <xsl:variable name="listtype">
      <xsl:value-of select="normalize-space(@type)"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$listtype = 'table'">
        <xsl:variable name="twoColumns">
          <xsl:value-of select="listheader/term"/>
        </xsl:variable>
        <xsl:text>&#10;</xsl:text>
        <xsl:choose>
          <xsl:when test="listheader">
            <xsl:if test="listheader/term">
              <xsl:text>|</xsl:text>
              <xsl:apply-templates select="listheader/term" />
            </xsl:if>
            <xsl:text>|</xsl:text>
            <xsl:apply-templates select="listheader/description" />
            <xsl:text>|&#10;</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>|||&#10;</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>|---|---|&#10;</xsl:text>
        <xsl:for-each select="item">
          <xsl:choose>
            <xsl:when test="$twoColumns">
              <xsl:text>|</xsl:text>
              <xsl:value-of select="string(term)"/>
              <xsl:text>|</xsl:text>
              <xsl:value-of select="string(description)" />
              <xsl:text>|&#10;</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>|</xsl:text>
              <xsl:if test="term">
                <xsl:value-of select="concat(string(term),'-')"/>
              </xsl:if>
              <xsl:value-of select="string(description)" />
              <xsl:text>|&#10;</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>&#10;</xsl:text>
        <xsl:if test="listheader">
          <xsl:text>**</xsl:text>
            <xsl:if test="listheader/term">
              <xsl:value-of select="concat(string(listheader/term),'-')"/>
            </xsl:if>
            <xsl:value-of select="string(listheader/description)" />
          <xsl:text>**</xsl:text>
          <xsl:text>&#10;</xsl:text>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="$listtype = 'bullet'">
            <xsl:for-each select="item">
              <xsl:text>- </xsl:text>
              <xsl:apply-templates select="term" />
              <xsl:apply-templates select="description" />
              <xsl:text>&#10;</xsl:text>
            </xsl:for-each>
          </xsl:when>
          <xsl:when test="$listtype = 'number'">
            <xsl:for-each select="item">
              <xsl:text>1. </xsl:text>
              <xsl:apply-templates select="term" />
              <xsl:apply-templates select="description" />
              <xsl:text>&#10;</xsl:text>
            </xsl:for-each>
          </xsl:when>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="description">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="term">
    <xsl:apply-templates />
  </xsl:template>  

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
