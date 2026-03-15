# ☁️ Azure App Registration and Retention Policy Guide for Mail Archiver

[← Back to Documentation Index](Index.md)

## 📋 Overview

This guide provides comprehensive instructions for setting up Azure App Registration for email retrieval and configuring retention policies in the Mail Archiver application.

## 📚 Table of Contents

1. [Overview](#overview)
2. [Azure App Registration for Email Retrieval](#azure-app-registration-for-email-retrieval)
   - [Mode A: App Credentials (Application Permissions)](#mode-a-app-credentials-application-permissions)
   - [Mode B: OAuth2 Connect (Delegated Permissions)](#mode-b-oauth2-connect-delegated-permissions)
3. [Configure Mail Archiver M365 Account](#configure-mail-archiver-m365-account)
4. [Retention Policy Setup](#retention-policy-setup)
   - [Configure Retention Policy](#configure-retention-policy)

## 🌐 Overview

The Mail Archiver application supports Microsoft 365 (M365) accounts with two authentication modes:
- **App Credentials** (legacy): client credentials with application permissions
- **OAuth2 Connect**: authorization code + refresh token with delegated permissions

Additionally, it provides retention policy functionality to automatically delete emails from the mail server after a specified period, helping manage storage space while maintaining a complete archive.

## ☁️ Azure App Registration for Email Retrieval

### 🛠️ Prerequisites

- Administrative access to Microsoft Entra ID (Azure AD)
- A Microsoft 365 tenant with Exchange Online licenses

### 🔐 Mode A: App Credentials (Application Permissions)

### 🚀 Create App Registration

1. Navigate to the [Microsoft Entra Admin Center](https://entra.microsoft.com)
2. Sign in with your administrator account
3. In the left navigation pane, select **App registrations**
4. Click **+ New registration** at the top of the App registrations page
5. Fill in the following details:
   - **Name**: Enter a descriptive name (e.g., "Mail Archiver M365 Provider")
   - **Supported account types**: Select **Accounts in this organizational directory only** (Single tenant)
    - This is required for the client credentials flow used by Mail Archiver
   - **Redirect URI**: Leave this blank (not needed for client credentials flow)
6. Click **Register**

**Important**: Note down the following values from the **Overview** page:
- **Application (client) ID** - You'll need this as `ClientId` in Mail Archiver
- **Directory (tenant) ID** - You'll need this as `TenantId` in Mail Archiver

### 🔐 Set Required API Permissions

1. Navigate to **API permissions** in the left menu
2. Click **+ Add a permission**
3. Select **Microsoft Graph**
4. Choose **Application permissions** (required for App Credentials mode)
5. Add the following permissions:
   - **Mail.Read** - Read mail in all mailboxes
   - **Mail.ReadWrite** - Read and write mail in all mailboxes (for restore function as well as deletion)

6. Click **Add permissions**
7. **CRITICAL**: Click **Grant admin consent for [Your Organization]**
8. Confirm by clicking **Yes**

**Note**: Application permissions are required because App Credentials mode accesses mailboxes without interactive user login.

### 🔑 Generate Client Secret

1. Navigate to **Certificates & secrets** in the left menu
2. Under **Client secrets**, click **+ New client secret**
3. Provide a description (e.g., "Mail Archiver Secret")
4. Select an expiration period
5. Click **Add**
6. **Important**: Copy the **Value** immediately and store it securely. This secret will not be shown again.

### 🔑 Mode B: OAuth2 Connect (Delegated IMAP Permissions)

Use this mode when you want to connect the mailbox interactively in Mail Archiver using authorization code flow and store a refresh token.

1. Navigate to the [Microsoft Entra Admin Center](https://entra.microsoft.com) and open **App registrations**
2. Create a new registration (or use an existing one)
3. In **Authentication**, add the redirect URI used by Mail Archiver for M365 OAuth2 Connect (must match your running Mail Archiver URL/path)
4. In **API permissions**, add delegated IMAP permission:
   - API: **Office 365 Exchange Online**
   - Permission: `IMAP.AccessAsUser.All`
5. Ensure `offline_access` is available for refresh tokens (requested by Mail Archiver during OAuth2 sign-in)
6. Click **Grant admin consent** if your tenant policy requires admin approval for delegated permissions
6. In **Certificates & secrets**, create a client secret and save it securely

> ℹ️ Depending on tenant security policy, user consent may be restricted. In this case, an administrator must grant consent before users can complete the Connect flow.

## 📧 Configure Mail Archiver M365 Account

After completing the app registration, configure a M365 mail account in Mail Archiver and choose the authentication mode.

### 📋 Required Values from App Registration (Mode-dependent):

1. **App Credentials mode**
   - **Client ID**: Application (client) ID
   - **Client Secret**: Client secret value
   - **Tenant ID**: Directory (tenant) ID
2. **OAuth2 Connect mode (IMAP alternative)**
   - **Client ID** and **Client Secret** are read from Mail Archiver `M365OAuth` settings (appsettings/env vars)
   - Tenant selection comes from your authority/tenant configuration and interactive sign-in

### 🚀 Creating M365 Account in Mail Archiver:

1. Log into your Mail Archiver application
2. Navigate to **Mail Accounts** > **Create**
3. Fill in the following fields:
   - **Name**: Descriptive name for the account (e.g., "Sales Team M365")
   - **Email Address**: The target mailbox email address to archive
   - **Provider**: Select **M365**
   - **Authentication Mode**: choose **App Credentials** or **OAuth2 Connect**
   - For **App Credentials**: enter **Client ID**, **Client Secret**, and **Tenant ID**

4. Click **Create**
5. If you selected **OAuth2 Connect**, open the account with **Edit** and click **Connect** to complete Microsoft sign-in and consent

### ⚠️ Important Notes:

- The **Email Address** field must contain the actual mailbox you want to archive
- The app registration must have permissions to access the specified mailbox

## 🗑️ Retention Policy Setup

The Mail Archiver application provides retention policy functionality to automatically delete emails from the IMAP server after they have been successfully archived, helping manage storage space on the IMAP server while maintaining a complete archive.

### 🛠️ Configure Retention Policy

1. Navigate to **Mail Accounts** in the Mail Archiver application
2. Either create a new M365 account or edit an existing one
3. In the account configuration form, locate the **Delete After Days** field
4. Enter the number of days after which archived emails should be deleted from the server:
   - For example, enter `30` to delete emails after 30 days
   - Leave empty to disable automatic deletion
5. Save the account configuration


## ℹ️ Information Regarding Migrated Mailboxes

### 📁 Folders Not Visible After Migration

If you have migrated your mailbox from another email provider to Microsoft 365, some folders may not appear in the folder list.

#### 🔍 Background

Mail Archiver retrieves folder information from M365 mailboxes via the Microsoft Graph API. The application correctly requests all available folders (including hidden folders) and traverses the complete folder hierarchy.

However, the Microsoft Graph API returns only folders with `PR_CONTAINER_CLASS` set to `IPF.Note`. During mailbox migrations from other email systems, folders may be assigned a different container class such as `IPF.Imap`. These folders are not exposed through the Graph API's folder listing endpoints.

This behavior affects all applications using this API for folder retrieval.

#### 🔧 Workaround Options

**Option 1: Use MFCMAPI Tool (Recommended)**

1. Download [MFCMAPI](https://github.com/Microsoft/mfcmapi/) from Microsoft's official repository
2. Launch MFCMAPI and connect to your mailbox
3. Navigate to each missing folder
4. Change the `PR_CONTAINER_CLASS` property from `IPF.Imap` to `IPF.Note`
5. Restart Outlook and reload the folders in Mail Archiver

**Option 2: Recreate Folders in Outlook**

1. Create new folders directly in Outlook (these will have the correct `IPF.Note` class)
2. Move all emails from the migrated folders to the new folders
3. Delete the old migrated folders if desired

**Option 3: PowerShell Script (For Bulk Changes)**

For mailboxes with many migrated folders, you can use PowerShell with Exchange Online module to change the container class for multiple folders at once. Contact your Exchange administrator for assistance with this approach.


---

**Note**: This guide is current as of 2025. Microsoft regularly updates their services, so some UI elements may differ. Always refer to the latest Microsoft documentation for the most up-to-date information.
