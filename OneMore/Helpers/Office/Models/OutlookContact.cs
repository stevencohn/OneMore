//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using System;
	using Microsoft.Office.Interop.Outlook;


	internal class OutlookContact
	{
		private readonly ContactItem contact;

		public OutlookContact(ContactItem contact)
		{
			this.contact = contact;
		}


		public ContactItem Contact => contact;


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

		public string Title
		{
			get => contact.Title;
			set { contact.Title = value; }
		}

		public string Email1Address
		{
			get => contact.Email1Address;
			set { contact.Email1Address = value; }
		}

		public string Email2Address
		{
			get => contact.Email2Address;
			set { contact.Email2Address = value; }
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
	}
}
