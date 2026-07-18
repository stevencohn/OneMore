//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using Microsoft.Office.Interop.Outlook;
	using System;
	using System.Drawing;
	using System.IO;
	using System.Runtime.InteropServices;

	internal class OutlookContact : IDisposable
	{
		private readonly ContactItem contact;
		private bool disposed;

		public OutlookContact(ContactItem contact)
		{
			this.contact = contact;
		}


		/// <summary>
		/// Releases the wrapped Outlook ContactItem COM object.
		/// </summary>
		public void Dispose()
		{
			if (disposed)
			{
				return;
			}

			Marshal.ReleaseComObject(contact);
			disposed = true;
		}


		public ContactItem Contact => contact;


		public string EntryID => contact.EntryID;


		//public string Folder => ((MAPIFolder)contact.Parent).Name;


		public string FirstName
		{
			get => contact.FirstName;
			set { contact.FirstName = value; }
		}

		public string MiddleName
		{
			get => contact.MiddleName;
			set { contact.MiddleName = value; }
		}

		public string LastName
		{
			get => contact.LastName;
			set { contact.LastName = value; }
		}

		public string CompanyName
		{
			get => contact.CompanyName;
			set { contact.CompanyName = value; }
		}

		public string Department
		{
			get => contact.Department;
			set { contact.Department = value; }
		}

		public string JobTitle
		{
			get => contact.JobTitle;
			set { contact.JobTitle = value; }
		}


		public string CustomerID
		{
			get => contact.CustomerID;
			set { contact.CustomerID = value; }
		}

		public string Email1Address
		{
			get => contact.Email1AddressType == "SMTP" ? contact.Email1Address : null;
			set
			{
				if (contact.Email1AddressType is null || contact.Email1AddressType == "SMTP")
				{
					contact.Email1Address = value;
				}
			}
		}

		public string Email2Address
		{
			get => contact.Email2AddressType == "SMTP" ? contact.Email2Address : null;
			set
			{
				if (contact.Email2AddressType is null || contact.Email2AddressType == "SMTP")
				{
					contact.Email2Address = value;
				}
			}
		}

		public string Email3Address
		{
			get => contact.Email3AddressType == "SMTP" ? contact.Email3Address : null;
			set
			{
				if (contact.Email3AddressType is null || contact.Email3AddressType == "SMTP")
				{
					contact.Email3Address = value;
				}
			}
		}

		public string BusinessTelephoneNumber
		{
			get => contact.BusinessTelephoneNumber;
			set { contact.BusinessTelephoneNumber = value; }
		}

		public string HomeTelephoneNumber
		{
			get => contact.HomeTelephoneNumber;
			set { contact.HomeTelephoneNumber = value; }
		}

		public string MobileTelephoneNumber
		{
			get => contact.MobileTelephoneNumber;
			set { contact.MobileTelephoneNumber = value; }
		}

		public string HomeAddressStreet
		{
			get => contact.HomeAddressStreet;
			set { contact.HomeAddressStreet = value; }
		}

		public string HomeAddressCity
		{
			get => contact.HomeAddressCity;
			set { contact.HomeAddressCity = value; }
		}

		public string HomeAddressState
		{
			get => contact.HomeAddressState;
			set { contact.HomeAddressState = value; }
		} 
		
		public string HomeAddressPostalCode
		{
			get => contact.HomeAddressPostalCode;
			set { contact.HomeAddressPostalCode = value; }
		}

		public string HomeAddressCountry
		{
			get => contact.HomeAddressCountry;
			set { contact.HomeAddressCountry = value; }
		}

		public string BusinessAddressStreet
		{
			get => contact.BusinessAddressStreet;
			set { contact.BusinessAddressStreet = value; }
		}

		public string BusinessAddressCity
		{
			get => contact.BusinessAddressCity;
			set { contact.BusinessAddressCity = value; }
		}

		public string BusinessAddressState
		{
			get => contact.BusinessAddressState;
			set { contact.BusinessAddressState = value; }
		}

		public string BusinessAddressPostalCode
		{
			get => contact.BusinessAddressPostalCode;
			set { contact.BusinessAddressPostalCode = value; }
		}

		public string BusinessAddressCountry
		{
			get => contact.BusinessAddressCountry;
			set { contact.BusinessAddressCountry = value; }
		}

		public DateTime Birthday
		{
			get => contact.Birthday;
			set { contact.Birthday = value; }
		}

		public DateTime Anniversary
		{
			get => contact.Anniversary;
			set { contact.Anniversary = value; }
		}


		public string Categories
		{
			get => contact.Categories;
			set { contact.Categories = value; }
		}


		public string Body
		{
			get => contact.Body;
			set { contact.Body = value; }
		}


		public Image Picture
		{
			get
			{
				if (contact.HasPicture)
				{
					var attachments = contact.Attachments;
					try
					{
						foreach (Attachment att in attachments)
						{
							try
							{
								// well-known picture attachment filename
								if (att.FileName.Equals(
									"ContactPicture.jpg", StringComparison.OrdinalIgnoreCase))
								{
									var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
									att.SaveAsFile(path);
									return Image.FromFile(path);
								}
							}
							finally
							{
								Marshal.ReleaseComObject(att);
							}
						}
					}
					finally
					{
						Marshal.ReleaseComObject(attachments);
					}
				}

				return null;
			}
		}


		/// <summary>
		/// Gets or sets the URI of the individual contact details subpage.
		/// </summary>
		public string PageUri { get; set; }
	}
}
