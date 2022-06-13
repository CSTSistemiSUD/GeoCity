Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data
Imports System.Web.Script.Services

' Per consentire la chiamata di questo servizio Web dallo script utilizzando ASP.NET AJAX, rimuovere il commento dalla riga seguente.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")>
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ScriptService()>
Public Class ws_Main
    Inherits System.Web.Services.WebService


#Region "Elenchi"
    Public Shared Function Fn_EstraiElenchi() As DatabaseOperationResult
        Dim queryAO As String = "SELECT AO_ID as [value], AO_DESC as [text] FROM TD_AREE_OMOGENEE"
        Dim queryAI As String = "SELECT AI_ID as [value], AI_DESC as [text] FROM TD_AREE_INTERVENTO"
        Dim queryUSO As String = "SELECT USO_ID, USO FROM AMB_USO"
        Dim queryTIPO As String = "SELECT USO_ID,TIPO_ID, TIPO FROM AMB_TIPO"
        Dim sqlCmdList As New List(Of SqlClient.SqlCommand)
        sqlCmdList.Add(New SqlClient.SqlCommand With {.CommandText = queryAO, .CommandType = CommandType.Text})
        sqlCmdList.Add(New SqlClient.SqlCommand With {.CommandText = queryAI, .CommandType = CommandType.Text})
        sqlCmdList.Add(New SqlClient.SqlCommand With {.CommandText = queryUSO, .CommandType = CommandType.Text})
        sqlCmdList.Add(New SqlClient.SqlCommand With {.CommandText = queryTIPO, .CommandType = CommandType.Text})
        Dim ssh As New SSHelper
        Return ssh.GetData(sqlCmdList)
    End Function

    <WebMethod>
    Public Function EstraiElenchi() As String
        Dim dbResult As DatabaseOperationResult = Fn_EstraiElenchi()
        Return dbResult.Serialize
    End Function

    Public Shared Function Fn_EstraiElenchiPerStruttura(PRJDATABASE As String, PRJITEMGUID As String) As DatabaseOperationResult
        Dim queryGerarchica = String.Format("SELECT DISTINCT
               amb.[AREA_OMOGENEA] AS AO_ID,
               ao.AO_DESC,
               amb.[AREA_INTERVENTO] AS AI_ID,
               ai.AI_DESC,
               tipo.USO_ID AS USO_ID,
               uso.uso AS USO,
               amb.[TipoUso] AS TIPO_ID,
               tipo.TIPO
        FROM {0}.[dbo].[FC_AMBIENTI] amb
             INNER JOIN {1}.dbo.TD_AREE_OMOGENEE ao ON amb.AREA_OMOGENEA = ao.AO_ID
             INNER JOIN {1}.dbo.TD_AREE_INTERVENTO ai ON amb.AREA_INTERVENTO = ai.AI_ID
             INNER JOIN {1}.dbo.AMB_USO AS uso ON amb.[uso] = uso.USO_ID
             INNER JOIN {1}.dbo.AMB_TIPO AS tipo ON amb.[TipoUso] = tipo.TIPO_ID
        WHERE amb.PRJITEMGUID = @PRJITEMGUID;", PRJDATABASE, ConfigurationManager.AppSettings("App_Database"))

        Dim sqlCmd As New SqlClient.SqlCommand With {.CommandText = queryGerarchica, .CommandType = CommandType.Text}
        sqlCmd.Parameters.AddWithValue("@PRJITEMGUID", PRJITEMGUID)
        Dim ssh As New SSHelper
        Return ssh.GetData(sqlCmd)

        'Dim ssh As New SSHelper
        'Return ssh.GetData(sqlCmdList)
        'Dim queryAO As String = String.Format("SELECT distinct amb.[AREA_OMOGENEA] as [value], ao.AO_DESC as [text] 
        'FROM {0}.[dbo].[FC_AMBIENTI] amb inner join {1}.dbo.TD_AREE_OMOGENEE ao
        'on amb.AREA_OMOGENEA = ao.AO_ID where amb.PRJITEMGUID=@PRJITEMGUID", PRJDATABASE, ConfigurationManager.AppSettings("App_Database"))


        'Dim queryAI As String = String.Format("SELECT distinct amb.[AREA_INTERVENTO] as [value], ai.AI_DESC as [text] 
        'FROM {0}.[dbo].[FC_AMBIENTI] amb inner join {1}.dbo.TD_AREE_INTERVENTO ai
        'on amb.AREA_INTERVENTO = ai.AI_ID where amb.PRJITEMGUID=@PRJITEMGUID", PRJDATABASE, ConfigurationManager.AppSettings("App_Database"))


        'Dim queryUSO As String = String.Format("SELECT distinct amb.[USO] AS USO_ID, uso.uso as USO 
        'FROM {0}.[dbo].[FC_AMBIENTI] amb inner join {1}.dbo.AMB_USO as uso
        'on amb.[uso] = uso.USO_ID where amb.PRJITEMGUID=@PRJITEMGUID", PRJDATABASE, ConfigurationManager.AppSettings("App_Database"))

        'Dim queryTIPO As String = String.Format("SELECT distinct amb.[TipoUso] AS TIPO_ID, tipo.USO_ID as USO_ID , tipo.TIPO 
        'FROM {0}.[dbo].[FC_AMBIENTI] amb inner join {1}.dbo.AMB_TIPO as tipo
        'on amb.[TipoUso] = tipo.TIPO_ID where amb.PRJITEMGUID=@PRJITEMGUID", PRJDATABASE, ConfigurationManager.AppSettings("App_Database"))

        'Dim sqlCmdList As New List(Of SqlClient.SqlCommand)
        'sqlCmdList.Add(New SqlClient.SqlCommand With {.CommandText = queryAO, .CommandType = CommandType.Text})
        'sqlCmdList.Add(New SqlClient.SqlCommand With {.CommandText = queryAI, .CommandType = CommandType.Text})
        'sqlCmdList.Add(New SqlClient.SqlCommand With {.CommandText = queryUSO, .CommandType = CommandType.Text})
        'sqlCmdList.Add(New SqlClient.SqlCommand With {.CommandText = queryTIPO, .CommandType = CommandType.Text})
        'For Each sqlCmd As SqlClient.SqlCommand In sqlCmdList
        '    sqlCmd.Parameters.AddWithValue("@PRJITEMGUID", PRJITEMGUID)
        'Next

        'Dim ssh As New SSHelper
        'Return ssh.GetData(sqlCmdList)
    End Function

    <WebMethod>
    Public Function EstraiElenchiPerStruttura(PRJDATABASE As String, PRJITEMGUID As String) As String

        Dim dbResult As DatabaseOperationResult = Fn_EstraiElenchiPerStruttura(PRJDATABASE, PRJITEMGUID)
        Return dbResult.Serialize
    End Function

#End Region


End Class