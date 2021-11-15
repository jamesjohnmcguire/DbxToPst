﻿/////////////////////////////////////////////////////////////////////////////
// <copyright file="DbxFolderIndexedItem.cs" company="James John McGuire">
// Copyright © 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;

namespace DbxToPstLibrary
{
	/// <summary>
	/// Represents a dbx folder indexed item.
	/// </summary>
	public class DbxFolderIndexedItem : DbxIndexedItem
	{
		/// <summary>
		/// The file name index of the folder.
		/// </summary>
		public const int FileName = 0x03;

		/// <summary>
		/// The flags index of the folder.
		/// </summary>
		public const int Flags = 0x06;

		/// <summary>
		/// The id index of the folder.
		/// </summary>
		public const int Id = 0x00;
		/// <summary>
		/// The name index of the folder.
		/// </summary>
		public const int Name = 0x02;

		/// <summary>
		/// The parent id index of the folder.
		/// </summary>
		public const int ParentId = 0x01;

		private DbxFolderIndex folderIndex;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="DbxFolderIndexedItem"/> class.
		/// </summary>
		/// <param name="fileBytes">The bytes of the file.</param>
		/// <param name="address">The address of the item with in
		/// the file.</param>
		public DbxFolderIndexedItem(byte[] fileBytes, uint address)
			: base(fileBytes, address)
		{
			folderIndex = new DbxFolderIndex();
		}

		public DbxFolderIndex FolderIndex { get { return folderIndex; } }

		/// <summary>
		/// Reads the indexed item and saves the values.
		/// </summary>
		/// <param name="fileBytes">The bytes of the file.</param>
		/// <param name="address">The address of the item with in
		/// the file.</param>
		public override void ReadIndex(byte[] fileBytes, uint address)
		{
			base.ReadIndex(fileBytes, address);

			folderIndex.FolderId = this.GetValue(Id);
			folderIndex.FolderParentId = this.GetValue(ParentId);
			folderIndex.FolderName = this.GetString(Name);
			folderIndex.FolderFileName = this.GetString(FileName);
		}
	}
}
