//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	internal static class ErrorCodes
	{
		public const uint hrCOMBusy = 0x8001010A;
		public const uint hrObjectMissing = 0x80042014;


		public static string GetDescription(int code)
		{
			return (uint)code switch
			{
				0x8001010A => "The message filter indicated that the application is busy",
				0x80042000 => "The XML is not well-formed",
				0x80042001 => "The XML is invalid",
				0x80042002 => "The section could not be created",
				0x80042003 => "The section could not be opened",
				0x80042004 => "The section does not exist",
				0x80042005 => "The page does not exist",
				0x80042006 => "The file does not exist",
				0x80042007 => "The image could not be inserted",
				0x80042008 => "The ink could not be inserted",
				0x80042009 => "The HTML could not be inserted",
				0x8004200a => "The page could not be opened",
				0x8004200b => "The section is read-only",
				0x8004200c => "The page is read-only",
				0x8004200d => "The outline text could not be inserted",
				0x8004200e => "The page object does not exist",
				0x8004200f => "The binary object does not exist",
				0x80042010 => "The last modified date does not match",
				0x80042011 => "The section group does not exist",
				0x80042012 => "The page does not exist in the section group",
				0x80042013 => "There is no active selection",
				0x80042014 => "The object does not exist",
				0x80042015 => "The notebook does not exist",
				0x80042016 => "The file could not be inserted",
				0x80042017 => "The name is invalid",
				0x80042018 => "The folder (section group) does not exist",
				0x80042019 => "The query is invalid",
				0x8004201a => "The file already exists",
				0x8004201b => "The section is encrypted and locked",
				0x8004201c => "The action is disabled by a policy",
				0x8004201d => "OneNote has not yet synchronized content",
				0x8004201E => "The section is from OneNote 2007 or earlier",
				0x8004201F => "The merge operation failed",
				0x80042020 => "The XML Schema is invalid",
				0x80042022 => "Content loss has occurred (from future versions of OneNote)",
				0x80042023 => "The action timed out",
				0x80042024 => "Audio recording is in progress",
				0x80042025 => "The linked-note state is unknown",
				0x80042026 => "No short name exists for the linked note",
				0x80042027 => "No friendly name exists for the linked note",
				0x80042028 => "The linked note URI is invalid",
				0x80042029 => "The linked note thumbnail is invalid",
				0x8004202A => "The importation of linked note thumbnail failed",
				0x8004202B => "Unread highlighting is disabled for the notebook",
				0x8004202C => "The selection is invalid",
				0x8004202D => "The conversion failed",
				0x8004202E => "Edit failed in the Recycle Bin",
				0x8004202F => "UpdatePageContent IMConversationType page node property was to a value other than 0, 1, 2 or 3",
				0x80042030 => "A modal dialog is blocking the app",
				_ => $"Unrecognized error code 0x{code:X}",
			};
		}
	}
}
