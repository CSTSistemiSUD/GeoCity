Option Strict Off
Option Explicit On

Imports System
Imports System.Web
Imports System.Web.Profile
Public Class ProfileCommon
    Inherits System.Web.Profile.ProfileBase
    <SettingsAllowAnonymous(False)>
    Public Overridable Property AziendaNazione() As String
        Get
            Return CType(Me.GetPropertyValue("AziendaNazione"), String)
        End Get
        Set(value As String)
            Me.SetPropertyValue("AziendaNazione", value)
        End Set
    End Property

    <SettingsAllowAnonymous(False)>
    Public Overridable Property AziendaPIva() As String
        Get
            Return CType(Me.GetPropertyValue("AziendaPIva"), String)
        End Get
        Set(value As String)
            Me.SetPropertyValue("AziendaPIva", value)
        End Set
    End Property

    <SettingsAllowAnonymous(False)>
    Public Overridable Property UtenteCodFis() As String
        Get
            Return CType(Me.GetPropertyValue("UtenteCodFis"), String)
        End Get
        Set(value As String)
            Me.SetPropertyValue("UtenteCodFis", value)
        End Set
    End Property

    <SettingsAllowAnonymous(False)>
    Public Overridable Property Nome() As String
        Get
            Return CType(Me.GetPropertyValue("Nome"), String)
        End Get
        Set(value As String)
            Me.SetPropertyValue("Nome", value)
        End Set
    End Property
    <SettingsAllowAnonymous(False)>
    Public Overridable Property Cognome() As String
        Get
            Return CType(Me.GetPropertyValue("Cognome"), String)
        End Get
        Set(value As String)
            Me.SetPropertyValue("Cognome", value)
        End Set
    End Property
    Public Shared Function GetProfile(ByVal username As String) As ProfileCommon
        Return CType(ProfileBase.Create(username), ProfileCommon)

    End Function
End Class
