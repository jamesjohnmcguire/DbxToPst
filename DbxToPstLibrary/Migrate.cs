﻿/////////////////////////////////////////////////////////////////////////////
// <copyright file="Migrate.cs" company="James John McGuire">
// Copyright © 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using DigitalZenWorks.Email.DbxOutlookExpress;
using Microsoft.Office.Interop.Outlook;
using MsgKit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

[assembly: CLSCompliant(false)]

namespace DbxToPstLibrary
{
	/// <summary>
	/// The transfer folder call back type.
	/// </summary>
	/// <param name="id">The folder id.</param>
	public delegate void TransferFolderCallBackType(int id);

	/// <summary>
	/// Migrate Dbx to Pst class.
	/// </summary>
	public static class Migrate
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Dbx directory to pst.
		/// </summary>
		/// <param name="dbxFoldersPath">The path to dbx folders to
		/// migrate.</param>
		/// <param name="pstPath">The path to pst file to copy to.</param>
		public static void DbxDirectoryToPst(
			string dbxFoldersPath, string pstPath)
		{
			// Personal preference... For me, most of these types will
			// likely be Japansese.
			Encoding.RegisterProvider(
				CodePagesEncodingProvider.Instance);
			Encoding encoding = Encoding.GetEncoding("shift_jis");

			DbxSet dbxSet = new (dbxFoldersPath, encoding);
			DbxFolder dbxFolder;

			PstOutlook pstOutlook = new ();
			Store pstStore = pstOutlook.CreateStore(pstPath);

			MAPIFolder rootFolder = pstStore.GetRootFolder();

			FileInfo fileInfo = new FileInfo(pstPath);
			rootFolder.Name = fileInfo.Name;

			IDictionary<uint, string> mappings =
				new Dictionary<uint, string>();

			do
			{
				dbxFolder = dbxSet.GetNextFolder();

				CopyFolderToPst(
					mappings, pstOutlook, pstStore, rootFolder, dbxFolder);
			}
			while (dbxFolder != null);
		}

		/// <summary>
		/// Dbx files to pst.
		/// </summary>
		/// <param name="filePath">The file path to migrate.</param>
		public static void DbxFileToPst(string filePath)
		{
			// Personal preference... For me, most of these types will
			// likely be Japansese.
			Encoding.RegisterProvider(
				CodePagesEncodingProvider.Instance);
			Encoding encoding = Encoding.GetEncoding("shift_jis");

			DbxSet dbxSet = new (filePath, encoding);
		}

		/// <summary>
		/// Dbx to pst.
		/// </summary>
		/// <param name="path">the path of the dbx element.</param>
		/// <returns>A value indicating success or not.</returns>
		/// <param name="pstPath">The path to pst file to copy to.</param>
		public static bool DbxToPst(string path, string pstPath)
		{
			bool result = false;

			if (Directory.Exists(path))
			{
				DbxDirectoryToPst(path, pstPath);
				result = true;
			}
			else if (File.Exists(path))
			{
				DbxFileToPst(path);
				result = true;
			}
			else
			{
				Log.Error("Invalid path");
			}

			return result;
		}

		private static MAPIFolder CopyChildFolderToPst(
			IDictionary<uint, string> mappings,
			PstOutlook pstOutlook,
			Store pstStore,
			DbxFolder dbxFolder)
		{
			MAPIFolder pstFolder = null;

			// need to figure out parent in pst
			bool keyExists =
				mappings.ContainsKey(dbxFolder.FolderParentId);

			if (keyExists == false)
			{
				Log.Warn("Parent key not found in mappings: " +
					dbxFolder.FolderParentId);
			}
			else
			{
				string entryId = mappings[dbxFolder.FolderParentId];
				MAPIFolder parentFolder =
					pstOutlook.GetFolderFromID(entryId, pstStore);

				pstFolder = PstOutlook.AddFolderSafe(
					parentFolder, dbxFolder.FolderName);
			}

			return pstFolder;
		}

		private static void CopyFolderToPst(
			IDictionary<uint, string> mappings,
			PstOutlook pstOutlook,
			Store pstStore,
			MAPIFolder rootFolder,
			DbxFolder dbxFolder)
		{
			if (dbxFolder != null)
			{
				MAPIFolder pstFolder;

				// add folder to pst
				if (dbxFolder.FolderParentId == 0)
				{
					// top level folder
					pstFolder = PstOutlook.AddFolderSafe(
						rootFolder, dbxFolder.FolderName);
				}
				else
				{
					pstFolder = CopyChildFolderToPst(
						mappings,
						pstOutlook,
						pstStore,
						dbxFolder);
				}

				if (pstFolder != null)
				{
					mappings.Add(dbxFolder.FolderId, pstFolder.EntryID);
				}

				// for each message
				DbxMessage dbxMessage;

				do
				{
					dbxMessage = dbxFolder.GetNextMessage();

					CopyMessageToPst(pstOutlook, pstFolder, dbxMessage);
				}
				while (dbxMessage != null);
			}
		}

		private static void CopyMessageToPst(
			PstOutlook pstOutlook, MAPIFolder pstFolder, DbxMessage dbxMessage)
		{
			if (dbxMessage != null)
			{
				// Need to get the rfc email as a stream, then
				// convert the stream to a MSG file, import the
				// MSG file into the Pst, finally move the message
				using Stream emailStream = dbxMessage.MessageStream;

				string msgFile = Path.GetTempFileName();

				// A 0 byte sized file is created.  Need to remove it.
				File.Delete(msgFile);
				msgFile = Path.ChangeExtension(msgFile, ".msg");

				using Stream msgStream =
					PstOutlook.GetMsgFileStream(msgFile);

				Converter.ConvertEmlToMsg(emailStream, msgStream);

				msgStream.Dispose();

				pstOutlook.AddMsgFile(pstFolder, msgFile);

				File.Delete(msgFile);
			}
		}
	}
}
