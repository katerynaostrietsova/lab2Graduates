<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

 
  <xsl:output method="html" encoding="utf-8" indent="yes" />

  
  <xsl:template match="/graduates">
    <html>
      <head>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <title>Облік випускників</title>
        <style type="text/css">
          body {
            font-family: Segoe UI, Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 20px;
          }
          h1 {
            text-align: center;
            margin-bottom: 10px;
          }
          .summary {
            text-align: center;
            margin-bottom: 20px;
            color: #555;
          }
          .card {
            background-color: #ffffff;
            border-radius: 6px;
            padding: 12px 16px;
            margin-bottom: 10px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.12);
          }
          .name {
            font-weight: bold;
            font-size: 1.1em;
            margin-bottom: 4px;
          }
          .meta {
            font-size: 0.95em;
            color: #333;
            margin-bottom: 4px;
          }
          .label {
            font-weight: bold;
          }
          .career-title {
            font-weight: bold;
            margin-top: 6px;
          }
          ul.career {
            margin: 4px 0 0 18px;
            padding: 0;
          }
          ul.career li {
            margin-bottom: 2px;
          }
        </style>
      </head>
      <body>
        <h1>Облік випускників</h1>

        <div class="summary">
          Кількість випускників у файлі:
          <strong><xsl:value-of select="count(graduate)" /></strong>
        </div>

        
        <xsl:for-each select="graduate">
          <div class="card">
            
            <div class="name">
              <xsl:value-of select="@fullName" />
            </div>

            
            <div class="meta">
              
              <xsl:if test="string-length(@faculty) &gt; 0">
                <span class="label">Факультет:</span>
                <xsl:text> </xsl:text>
                <xsl:value-of select="@faculty" />
                <xsl:text>; </xsl:text>
              </xsl:if>

              
              <xsl:if test="string-length(@department) &gt; 0">
                <span class="label">Кафедра:</span>
                <xsl:text> </xsl:text>
                <xsl:value-of select="@department" />
                <xsl:text>; </xsl:text>
              </xsl:if>

              
              <xsl:if test="string-length(@speciality) &gt; 0">
                <span class="label">Спеціальність:</span>
                <xsl:text> </xsl:text>
                <xsl:value-of select="@speciality" />
                <xsl:text>; </xsl:text>
              </xsl:if>

              
              <xsl:if test="string-length(@group) &gt; 0">
                <span class="label">Група:</span>
                <xsl:text> </xsl:text>
                <xsl:value-of select="@group" />
                <xsl:text>; </xsl:text>
              </xsl:if>

              
              <xsl:if test="string-length(@admissionYear) &gt; 0 or string-length(@graduationYear) &gt; 0">
                <span class="label">Період навчання:</span>
                <xsl:text> </xsl:text>
                <xsl:value-of select="@admissionYear" />
                <xsl:if test="string-length(@admissionYear) &gt; 0 and string-length(@graduationYear) &gt; 0">
                  <xsl:text>–</xsl:text>
                </xsl:if>
                <xsl:value-of select="@graduationYear" />
              </xsl:if>
            </div>

            
            <xsl:if test="careerMove">
              <div class="career-title">Кар'єра:</div>
              <ul class="career">
                <xsl:for-each select="careerMove">
                  <li>
                    <xsl:value-of select="." />
                  </li>
                </xsl:for-each>
              </ul>
            </xsl:if>
          </div>
        </xsl:for-each>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
