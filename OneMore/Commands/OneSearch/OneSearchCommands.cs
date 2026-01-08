//************************************************************************************************
// OneSearch commands integrated into OneMore
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.OneSearch
{
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class OneSearchCommand : Command
	{
		public override async Task Execute(params object[] args)
		{
			var service = new OneSearchService(logger);
			await StaTask.Run(() =>
			{
				using var dialog = new OneSearchForm(service)
				{
					StartPosition = FormStartPosition.CenterScreen
				};
				dialog.ShowDialog();
				return 0;
			});
		}
	}


	internal class OneSearchSyncCommand : Command
	{
		public override async Task Execute(params object[] args)
		{
			var service = new OneSearchService(logger);
			var settings = service.LoadSettings();

			if (string.IsNullOrWhiteSpace(settings.CacheRoot))
			{
				MessageBox.Show(
					Resx.OneSearch_MissingCache,
					Resx.OneSearch_Title,
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			await StaTask.Run(() =>
			{
				using var dialog = new OneSearchSyncDialog(service, settings);
				dialog.ShowDialog();
				return 0;
			});
		}
	}


	internal class OneSearchClearCacheCommand : Command
	{
		public override async Task Execute(params object[] args)
		{
			try
			{
				var service = new OneSearchService(logger);
				var settings = service.LoadSettings();

				if (string.IsNullOrWhiteSpace(settings.CacheRoot))
				{
					MessageBox.Show(
						Resx.OneSearch_MissingCache,
						Resx.OneSearch_Title,
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
					return;
				}

				service.ClearCache(settings.CacheRoot);
				MessageBox.Show(
					Resx.OneSearch_CacheCleared,
					Resx.OneSearch_Title,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (System.Exception exc)
			{
				logger?.WriteLine("OneSearch clear cache failed", exc);
				MessageBox.Show(exc.Message, Resx.OneSearch_Title);
			}

			await Task.Yield();
		}
	}


	internal class OneSearchSetCacheCommand : Command
	{
		public override async Task Execute(params object[] args)
		{
			var service = new OneSearchService(logger);
			var settings = service.LoadSettings();

			await StaTask.Run(() =>
			{
				using var dialog = new FolderBrowserDialog
				{
					SelectedPath = settings.CacheRoot
				};

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					settings.CacheRoot = dialog.SelectedPath;
					service.SaveSettings(settings);
					MessageBox.Show(
						Resx.OneSearch_CacheSet,
						Resx.OneSearch_Title,
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}

				return 0;
			});
		}
	}
}
