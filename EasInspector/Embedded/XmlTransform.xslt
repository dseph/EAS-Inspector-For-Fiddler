<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output indent="yes" method="html" doctype-public="-//W3C//DTD HTML 4.01//EN" />

  <xsl:template match="/">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="processing-instruction()">
    <div class="e">
      <span class="b">
        <xsl:call-template name="entity-ref">
          <xsl:with-param name="name">nbsp</xsl:with-param>
        </xsl:call-template>
      </span>
      <span class="m">
        <xsl:text>&lt;?</xsl:text>
      </span>
      <span class="pi">
        <xsl:value-of select="name(.)"/>
        <xsl:value-of select="."/>
      </span>
      <span class="m">
        <xsl:text>?></xsl:text>
      </span>
    </div>
  </xsl:template>

  <xsl:template match="processing-instruction('xml')">
    <div class="e">
      <span class="b">
        <xsl:call-template name="entity-ref">
          <xsl:with-param name="name">nbsp</xsl:with-param>
        </xsl:call-template>
      </span>
      <span class="m">
        <xsl:text>&lt;?</xsl:text>
      </span>
      <span class="pi">
        <xsl:text>xml </xsl:text>
        <xsl:for-each select="@*">
          <xsl:value-of select="name(.)"/>
          <xsl:text>="</xsl:text>
          <xsl:value-of select="."/>
          <xsl:text>" </xsl:text>
        </xsl:for-each>
      </span>
      <span class="m">
        <xsl:text>?></xsl:text>
      </span>
    </div>
  </xsl:template>

  <!-- This is where we deal with attributes -->
  <xsl:template match="@*">
    <xsl:choose>
      <xsl:when test="contains(name(.), 'DocRef')">
        <xsl:call-template name="DocRef"/>
      </xsl:when>
      <xsl:otherwise>
        <span>
          <xsl:attribute name="class">
            <xsl:if test="xsl:*/@*">
              <xsl:text>x</xsl:text>
            </xsl:if>
            <xsl:text>t</xsl:text>
          </xsl:attribute>
          <xsl:call-template name="entity-ref">
            <xsl:with-param name="name">nbsp</xsl:with-param>
          </xsl:call-template>
          <xsl:value-of select="name(.)"/>
        </span>
        <span class="m">="</span>
        <B>
          <xsl:value-of select="."/>
        </B>
        <span class="m">"</span>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="text()">
    <div class="e">
      <span class="b"> </span>
      <span class="tx">
        <xsl:value-of select="."/>
      </span>
    </div>
  </xsl:template>

  <xsl:template match="comment()">
    <div class="k">
      <span>
        <A style="visibility:hidden" class="b" onclick="return false" onfocus="h()">-</A>
        <span class="comment">
          <xsl:text>&lt;!--</xsl:text>
        </span>
      </span>
      <span class="comment" id="clean">
        <PRE>
          <xsl:value-of select="."/>
        </PRE>
      </span>
      <span class="comment">
        <xsl:text>--></xsl:text>
      </span>
      <SCRIPT>f(clean);</SCRIPT>
    </div>
  </xsl:template>

  <xsl:template match="*">
    <div class="e">
      <div style="margin-left:1em;text-indent:-2em">
        <span class="b">
          <xsl:call-template name="entity-ref">
            <xsl:with-param name="name">nbsp</xsl:with-param>
          </xsl:call-template>
        </span>
        <span class="m">&lt;</span>
        <span>
          <xsl:attribute name="class">
            <xsl:if test="xsl:*">
              <xsl:text>x</xsl:text>
            </xsl:if>
            <xsl:text>t</xsl:text>
          </xsl:attribute>
          <xsl:value-of select="name(.)"/>
          <!--
          <xsl:if test="@*">
            <xsl:text> </xsl:text>
          </xsl:if>
          -->
        </span>
        <xsl:apply-templates select="@*"/>
        <span class="m">
          <xsl:text>/></xsl:text>
        </span>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="*[node()]">
    <div class="e">
      <div class="c">
        <A class="b" href="#" onclick="return false" onfocus="h()">-</A>
        <span class="m">&lt;</span>
        <span>
          <xsl:attribute name="class">
            <xsl:if test="xsl:*">
              <xsl:text>x</xsl:text>
            </xsl:if>
            <xsl:text>t</xsl:text>
          </xsl:attribute>
          <xsl:value-of select="name(.)"/>
          <xsl:if test="@*">
            <xsl:text> </xsl:text>
          </xsl:if>
        </span>
        <xsl:apply-templates select="@*"/>
        <span class="m">
          <xsl:text>></xsl:text>
        </span>
      </div>
      <div>
        <xsl:apply-templates/>
        <div>
          <span class="b">
            <xsl:call-template name="entity-ref">
              <xsl:with-param name="name">nbsp</xsl:with-param>
            </xsl:call-template>
          </span>
          <span class="m">
            <xsl:text>&lt;/</xsl:text>
          </span>
          <span>
            <xsl:attribute name="class">
              <xsl:if test="xsl:*">
                <xsl:text>x</xsl:text>
              </xsl:if>
              <xsl:text>t</xsl:text>
            </xsl:attribute>
            <xsl:value-of select="name(.)"/>
          </span>
          <span class="m">
            <xsl:text>></xsl:text>
          </span>
        </div>
      </div>
    </div>
  </xsl:template>

  <!-- This handles children with data in them who are not parents of other nodes -->
  <xsl:template match="*[text() and not (comment() or processing-instruction())]">
    <div class="e">
      <div style="margin-left:1em;text-indent:-2em">
        <span class="b">
          <xsl:call-template name="entity-ref">
            <xsl:with-param name="name">nbsp</xsl:with-param>
          </xsl:call-template>
        </span>
        <span class="m">
          <xsl:text>&lt;</xsl:text>
        </span>
        <span>
          <xsl:attribute name="class">
            <xsl:if test="xsl:*">
              <xsl:text>x</xsl:text>
            </xsl:if>
            <xsl:text>t</xsl:text>
          </xsl:attribute>
          <xsl:choose>
            <xsl:when test="contains(substring(name(.), string-length(name(.))-7, string-length(name(.))), 'InBytes')">
              <xsl:value-of select="substring(name(.), 0, string-length(name(.))-6)"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="name(.)"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="@*">
            <xsl:text> </xsl:text>
          </xsl:if>
        </span>
        <xsl:apply-templates select="@*"/>
        <span class="m">
          <xsl:text>></xsl:text>
        </span>
        <!-- Inner text of a node -->
        <xsl:choose>
          <xsl:when test="name(.) = 'MIME' or name(.) = 'Data'">
            <xsl:call-template name="MIME"/>
          </xsl:when>
          <xsl:otherwise>
            <span class="tx">
              <xsl:choose>
                <xsl:when test="contains(name(.), 'Url')">
                  <xsl:call-template name="Url"/>
                </xsl:when>
                <xsl:when test="contains(substring(name(.), string-length(name(.))-7, string-length(name(.))), 'InBytes')">
                  <xsl:value-of select="."/>
                  <xsl:text> bytes</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="."/>
                </xsl:otherwise>
              </xsl:choose>
            </span>
          </xsl:otherwise>
        </xsl:choose>
        <span class="m">&lt;/</span>
        <span>
          <xsl:attribute name="class">
            <xsl:if test="xsl:*">
              <xsl:text>x</xsl:text>
            </xsl:if>
            <xsl:text>t</xsl:text>
          </xsl:attribute>
          <xsl:choose>
            <xsl:when test="contains(substring(name(.), string-length(name(.))-7, string-length(name(.))), 'InBytes')">
              <xsl:value-of select="substring(name(.), 0, string-length(name(.))-6)"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="name(.)"/>
            </xsl:otherwise>
          </xsl:choose>
        </span>
        <span class="m">
          <xsl:text>></xsl:text>
        </span>
      </div>
    </div>
  </xsl:template>

  <!-- This handles the start of a parent -->
  <xsl:template match="*[*]" priority="20">
    <div class="e">
      <!--div style="margin-left:1em;text-indent:-2em" class="c"-->
      <div style="margin-left:1em;" class="c">
        <A class="b" href="#" onclick="return false" onfocus="h()">
          <xsl:choose>
            <xsl:when test="contains(name(.), 'ServerObjectHandleTable') or contains(name(.), 'PropertyTags') or (name(.) = 'PropertyRow') or (name(.) = 'Properties')">
              <!-- Minimized for easier readability -->
              <xsl:text>+</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>-</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </A>
        <span class="m">&lt;</span>
        <span>
          <xsl:attribute name="class">
            <xsl:if test="xsl:*">
              <xsl:text>x</xsl:text>
            </xsl:if>
            <xsl:text>t</xsl:text>
          </xsl:attribute>
          <xsl:value-of select="name(.)"/>
          <!--
                I'm not sure why this space is here instead of in the template.
                There are other spots with this, but I don't have examples yet
                of where they may need to be changed.
          -->
          <!--
          <xsl:if test="@*">
            <xsl:text> </xsl:text>
          </xsl:if>
          -->
        </span>
        <xsl:apply-templates select="@*"/>
        <span class="m">
          <xsl:text>></xsl:text>
        </span>
      </div>
      <div>
        <xsl:if test="contains(name(.), 'ServerObjectHandleTable') or contains(name(.), 'PropertyTags') or (name(.) = 'PropertyRow') or (name(.) = 'Properties')">
          <!-- Minimized for easier readability -->
          <xsl:attribute name="style">
            <xsl:text>display:none</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <xsl:apply-templates/>
        <div>
          <span class="b">
            <xsl:call-template name="entity-ref">
              <xsl:with-param name="name">nbsp</xsl:with-param>
            </xsl:call-template>
          </span>
          <span class="m">
            <xsl:text>&lt;/</xsl:text>
          </span>
          <span>
            <xsl:attribute name="class">
              <xsl:if test="xsl:*">
                <xsl:text>x</xsl:text>
              </xsl:if>
              <xsl:text>t</xsl:text>
            </xsl:attribute>
            <xsl:value-of select="name(.)"/>
          </span>
          <span class="m">
            <xsl:text>></xsl:text>
          </span>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template name="DocRef">
    <xsl:text> (</xsl:text>
    <a>
      <xsl:attribute name="class">
        <xsl:text>nojs</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="target">
        <xsl:text>_BLANK</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:value-of select="." disable-output-escaping="yes"/>
      </xsl:attribute>
      <xsl:text>DocRef</xsl:text>
    </a>
    <xsl:text>)</xsl:text>
  </xsl:template>

  <xsl:template name="MIME">
    <span class="m">
      <xsl:text>&lt;![CDATA[</xsl:text>
      <br/>
    </span>
    <code>
      <xsl:value-of select="." disable-output-escaping="yes"/>
    </code>
    <span class="m">
      <br/>
      <xsl:text>]]&gt;</xsl:text>
    </span>
  </xsl:template>

  <xsl:template name="Url">
    <a>
      <xsl:attribute name="class">
        <xsl:text>nojs</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="target">
        <xsl:text>_BLANK</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:value-of select="." disable-output-escaping="yes"/>
      </xsl:attribute>
      <xsl:value-of select="." disable-output-escaping="yes"/>
    </a>
  </xsl:template>

  <xsl:template name="entity-ref">
    <xsl:param name="name"/>
    <xsl:text disable-output-escaping="yes">&amp;</xsl:text>
    <xsl:value-of select="$name"/>
    <xsl:text>;</xsl:text>
  </xsl:template>

</xsl:stylesheet>