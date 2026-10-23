<?xml version="1.0" encoding="utf-8"?>
<!--
  Hairstyles_dlx_Extra - REVERSE (v2)
  Ванильные причёски ОСТАЮТСЯ на своих исходных местах (не трогаются),
  поэтому NPC по умолчанию генерятся ванильно, как в чистой игре.
  DLX-причёски добавлены в НОВЫЕ слоты с УРЕЗАННЫМИ культур-тегами:
    - мужские DLX: только empire + vlandia (западноевропейский вид)
    - женские DLX: только sturgia + nord (викингские косы)
  => DLX появляются у NPC заметно реже ванили и только у этих культур,
     но в character creator доступны все, для игрока.
-->
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output omit-xml-declaration="yes"/>

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match='race[@id="human"]/skin[@name="man"]/hair_meshes'>
		<xsl:copy>
			<xsl:copy-of select="@*"/>
			<xsl:copy-of select="node()"/>
			<hair_mesh name="hair_male_testpapito" cover_type4="hair_male_testpapito">
				<style_tags>
					<style_tag name="BraidedAndLong" />
					<style_tag name="empire" />
					<style_tag name="vlandia" />
				</style_tags>
			</hair_mesh>
		</xsl:copy>
	</xsl:template>

	<xsl:template match='race[@id="human"]/skin[@name="woman"]/hair_meshes'>
		<xsl:copy>
			<xsl:copy-of select="@*"/>
			<xsl:copy-of select="node()"/>
		</xsl:copy>
	</xsl:template>

</xsl:stylesheet>
