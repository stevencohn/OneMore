//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	internal static class ErrorCodes
	{
		public static string GetDescription(int code)
		{
			switch ((uint)code)
			{
				case 0x8001010A: return "The message filter indicated that the application is busy";
				case 0x80042000: return "The XML is not well-formed";
				case 0x80042001: return "The XML is invalid";
				case 0x80042002: return "The section could not be created";
				case 0x80042003: return "The section could not be opened";
				case 0x80042004: return "The section does not exist";
				case 0x80042005: return "The page does not exist";
				case 0x80042006: return "The file does not exist";
				case 0x80042007: return "The image could not be inserted";
				case 0x80042008: return "The ink could not be inserted";
				case 0x80042009: return "The HTML could not be inserted";
				case 0x8004200a: return "The page could not be opened";
				case 0x8004200b: return "The section is read-only";
				case 0x8004200c: return "The page is read-only";
				case 0x8004200d: return "The outline text could not be inserted";
				case 0x8004200e: return "The page object does not exist";
				case 0x8004200f: return "The binary object does not exist";
				case 0x80042010: return "The last modified date does not match";
				case 0x80042011: return "The section group does not exist";
				case 0x80042012: return "The page does not exist in the section group";
				case 0x80042013: return "There is no active selection";
				case 0x80042014: return "The object does not exist";
				case 0x80042015: return "The notebook does not exist";
				case 0x80042016: return "The file could not be inserted";
				case 0x80042017: return "The name is invalid";
				case 0x80042018: return "The folder (section group) does not exist";
				case 0x80042019: return "The query is invalid";
				case 0x8004201a: return "The file already exists";
				case 0x8004201b: return "The section is encrypted and locked";
				case 0x8004201c: return "The action is disabled by a policy";
				case 0x8004201d: return "OneNote has not yet synchronized content";
				case 0x8004201E: return "The section is from OneNote 2007 or earlier";
				case 0x8004201F: return "The merge operation failed";
				case 0x80042020: return "The XML Schema is invalid";
				case 0x80042022: return "Content loss has occurred (from future versions of OneNote)";
				case 0x80042023: return "The action timed out";
				case 0x80042024: return "Audio recording is in progress";
				case 0x80042025: return "The linked-note state is unknown";
				case 0x80042026: return "No short name exists for the linked note";
				case 0x80042027: return "No friendly name exists for the linked note";
				case 0x80042028: return "The linked note URI is invalid";
				case 0x80042029: return "The linked note thumbnail is invalid";
				case 0x8004202A: return "The importation of linked note thumbnail failed";
				case 0x8004202B: return "Unread highlighting is disabled for the notebook";
				case 0x8004202C: return "The selection is invalid";
				case 0x8004202D: return "The conversion failed";
				case 0x8004202E: return "Edit failed in the Recycle Bin";
				case 0x8004202F: return "UpdatePageContent IMConversationType page node property was to a value other than 0, 1, 2 or 3";
				case 0x80042030: return "A modal dialog is blocking the app";
			}

			return $"Unrecognized error code 0x{code:X}";
		}
	}
}
