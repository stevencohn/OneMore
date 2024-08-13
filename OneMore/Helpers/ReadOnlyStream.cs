//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.IO;
	using System.Runtime.InteropServices.ComTypes;
	using Marshal = System.Runtime.InteropServices.Marshal;


	//********************************************************************************************
	// class ReadOnlyStream
	//********************************************************************************************

	internal class ReadOnlyStream : IStream, IDisposable
	{
		private Stream stream;


		//========================================================================================
		// Lifecycle
		//========================================================================================

		/// <summary>
		/// Initialize a new instance, preserving a reference to the COM stream wrapper.
		/// </summary>
		/// <param name="wrapper">The COM Stream wrapper</param>

		public ReadOnlyStream(Stream wrapper)
		{
			stream = wrapper;
		}


		#region IDisposable Support
		private bool disposed = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (stream != null)
					{
						stream.Dispose();
						stream = null;
					}
				}

				disposed = true;
			}
		}


		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion


		//========================================================================================
		// IStream implementation
		//========================================================================================

		/// <summary>
		/// Creates a new stream object with its own seek pointer that references the
		/// same bytes as the original stream.
		/// </summary>
		/// <param name="ppstm">
		/// Upon return, contains the new stream object.
		/// </param>

		public void Clone(out IStream ppstm)
		{
			ppstm = new ReadOnlyStream(stream);
		}


		/// <summary>
		/// Ensures that any changes made to a stream object opened in transacted
		/// mode are reflected in the parent storage.
		/// </summary>
		/// <param name="grfCommitFlags">
		/// A value that controls how the changes for the stream object are committed.
		/// </param>

		public void Commit(int grfCommitFlags)
		{
			stream.Flush();
		}


		#region CopyTo()

		/// <summary>
		/// Copies a specified number of bytes from the current seek pointer in the stream
		/// to the current seek pointer in another stream.
		/// </summary>
		/// <param name="pstm">Destination stream.</param>
		/// <param name="cb">The number of bytes to copy from the source stream.</param>
		/// <param name="pcbRead">Upon return, the actual number of bytes read from the source.</param>
		/// <param name="pcbWritten">Upon return, the actual number of bytes written to the destination.</param>

		public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
		{
			// N/A
		}

		#endregion CopyTo

		#region LockRegion()

		/// <summary>
		/// Restricts access to a specified range of bytes in the stream.
		/// </summary>
		/// <param name="libOffset">The byte offset for the beginning of the range.</param>
		/// <param name="cb">The length of the range, in bytes, to restrict.</param>
		/// <param name="dwLockType">The requested restriction on accessing the range.</param>

		public void LockRegion(long libOffset, long cb, int dwLockType)
		{
			throw new NotSupportedException("ReadOnlyStream does not support LockRegion");
		}

		#endregion LockRegion



		/// <summary>
		/// Reads a specified number of bytes from the stream object into memory starting
		/// at the current seek pointer.
		/// </summary>
		/// <param name="pv">Upon return, contains the data read from the stream.</param>
		/// <param name="cb">The number of bytes to read from the stream object.</param>
		/// <param name="pcbRead">
		/// A pointer to a ULONG variable that receives the actual number of bytes read
		/// from the stream object.
		/// </param>

		public void Read(byte[] pv, int cb, IntPtr pcbRead)
		{
			Marshal.WriteInt64(pcbRead, (long)stream.Read(pv, 0, cb));
		}


		#region Revert()

		/// <summary>
		/// Discards all changes that have been made to a transacted stream since the
		/// last System.Runtime.InteropServices.ComTypes.IStream.Commit(System.Int32) call.
		/// </summary>

		public void Revert()
		{
			throw new NotSupportedException("ReadOnlyStream does not support Revert");
		}

		#endregion Revert


		/// <summary>
		/// Changes the seek pointer to a new location relative to the beginning of the
		/// stream, to the end of the stream, or to the current seek pointer.
		/// </summary>
		/// <param name="dlibMove">The displacement to add to to dwOrigin</param>
		/// <param name="dwOrigin">
		/// The origin of the seek. The origin can be the beginning of the file, the
		/// current seek pointer, or the end of the file.
		/// </param>
		/// <param name="plibNewPosition">
		/// Upon return, contains the offset of the seek pointer from the beginning of the stream.
		/// </param>

		public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
		{
			Marshal.WriteInt64(plibNewPosition, stream.Position);

			long num;
			switch (dwOrigin)
			{
				case 0: // STREAM_SEEK_SET
					num = dlibMove;
					break;

				case 1: // STREAM_SEEK_CUR
					num = stream.Position + dlibMove;
					break;

				case 2: // STREAM_SEEK_END
					num = stream.Length + dlibMove;
					break;

				default:
					return;
			}

			if ((num >= 0L) && (num < stream.Length))
			{
				stream.Position = num;
				Marshal.WriteInt64(plibNewPosition, stream.Position);
			}
		}


		/// <summary>
		/// Changes the size of the stream.
		/// </summary>
		/// <param name="libNewSize">The new size of the stream in bytes.</param>

		public void SetSize(long libNewSize)
		{
			stream.SetLength(libNewSize);
		}


		/// <summary>
		/// Retrieves the STATSTG structure for this stream.
		/// </summary>
		/// <param name="pstatstg">Upon return, contains the stat information.</param>
		/// <param name="grfStatFlag">
		/// Filter of members to not return in the struct thus saving memory.
		/// </param>

		public void Stat(out STATSTG pstatstg, int grfStatFlag)
		{
			pstatstg = new STATSTG
			{
				cbSize = stream.Length
			};

			if ((grfStatFlag & 0x0001 /* STATFLAG_NONAME */ ) == 0)
			{
				pstatstg.pwcsName = stream.ToString();
			}
		}


		#region UnlockRegion()

		/// <summary>
		/// Removes the access restriction on a range of bytes previously restricted
		/// with the LockRegion method.
		/// </summary>
		/// <param name="libOffset">The byte offset for the beginning of the stream.</param>
		/// <param name="cb">The length, in bytes, of the range to restrict.</param>
		/// <param name="dwLockType">The access restrictions previously placed on the range.</param>

		public void UnlockRegion(long libOffset, long cb, int dwLockType)
		{
			throw new NotSupportedException("ReadOnlyStream does not support UnlockRegion");
		}

		#endregion UnlockRegion


		/// <summary>
		/// Writes a specified number of bytes into the stream object starting at the
		/// current seek pointer.
		/// </summary>
		/// <param name="pv">The buffer to write this stream to.</param>
		/// <param name="cb">The number of bytes to write to the stream.</param>
		/// <param name="pcbWritten">
		/// Upon return, contains the actual number of bytes written to the stream object. 
		/// If the caller sets this pointer to System.IntPtr.Zero, this method does not provide
		/// the actual number of bytes written.
		/// </param>

		public void Write(byte[] pv, int cb, IntPtr pcbWritten)
		{
			Marshal.WriteInt64(pcbWritten, 0L);
			stream.Write(pv, 0, cb);
			Marshal.WriteInt64(pcbWritten, (long)cb);
		}
	}
}

