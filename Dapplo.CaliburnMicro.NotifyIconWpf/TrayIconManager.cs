﻿#region Dapplo 2016 - GNU Lesser General Public License

// Dapplo - building blocks for .NET applications
// Copyright (C) 2016 Dapplo
// 
// For more information see: http://dapplo.net/
// Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
// This file is part of Dapplo.CaliburnMicro
// 
// Dapplo.CaliburnMicro is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Dapplo.CaliburnMicro is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have a copy of the GNU Lesser General Public License
// along with Dapplo.CaliburnMicro. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Caliburn.Micro;
using Dapplo.Addons;
using Dapplo.Log.Facade;
using Dapplo.Utils;

#endregion

namespace Dapplo.CaliburnMicro.NotifyIconWpf
{
	/// <summary>
	///     This takes care of starting and managing your trayicons
	/// </summary>
	[StartupAction(StartupOrder = (int) CaliburnStartOrder.TrayIcons)]
	[ShutdownAction]
	[Export(typeof(ITrayIconManager))]
	public class TrayIconManager : IStartupAction, IShutdownAction, ITrayIconManager
	{
		private static readonly LogSource Log = new LogSource();

		/// <summary>
		///     Cache for the created tray icons
		/// </summary>
		private readonly IDictionary<WeakReference<ITrayIconViewModel>, WeakReference<ITrayIcon>> _trayIcons =
			new Dictionary<WeakReference<ITrayIconViewModel>, WeakReference<ITrayIcon>>();

		[ImportMany]
		private IEnumerable<ITrayIconViewModel> TrayIconViewModels { get; set; }

		[Import]
		private IWindowManager WindowsManager { get; set; }

		/// <summary>
		///     Hide all trayicons to prevent them hanging useless in the system tray
		/// </summary>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Task</returns>
		public async Task ShutdownAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var trayIcons = _trayIcons.Values.Select(x =>
			{
				ITrayIcon trayIcon;
				x.TryGetTarget(out trayIcon);
				return trayIcon;
			}).Where(x => x != null).ToList();

			if (trayIcons.Any())
			{
				Log.Debug().WriteLine("Hiding all created tray-icons");

				await UiContext.RunOn(() =>
				{
					Log.Debug().WriteLine("Hiding {0} tray-icons", trayIcons.Count());
					foreach (var trayIcon in trayIcons)
					{
						trayIcon.Hide();
					}
				}, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				Log.Debug().WriteLine("No tray-icons to hide");
			}
		}

		/// <summary>
		///     Find all trayicons and initialize them.
		/// </summary>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Task</returns>
		public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			await UiContext.RunOn(() =>
			{
				foreach (var trayIconViewModel in TrayIconViewModels)
				{
					// Get the view, to store it as ITrayIcon
					trayIconViewModel.ViewAttached += (sender, e) =>
					{
						var popup = e.View as Popup;
						var contentControl = e.View as ContentControl;
						var trayIcon = popup?.Child as ITrayIcon ?? contentControl?.Content as ITrayIcon ?? e.View as ITrayIcon;
						_trayIcons.Add(new WeakReference<ITrayIconViewModel>(trayIconViewModel), new WeakReference<ITrayIcon>(trayIcon));
					};
					WindowsManager.ShowPopup(trayIconViewModel);
				}
			}, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		///     Get the ITrayIcon belonging to the specified ITrayIconViewModel instance
		/// </summary>
		/// <param name="trayIconViewModel">ViewModel instance to get the ITrayIcon for</param>
		/// <returns>ITrayIcon</returns>
		public ITrayIcon GetTrayIconFor(ITrayIconViewModel trayIconViewModel)
		{
			var result = _trayIcons.Where(x =>
			{
				// Try to get the key to compare
				ITrayIconViewModel currentTrayIconViewModel;
				x.Key.TryGetTarget(out currentTrayIconViewModel);
				// Try to get the value to check if it's available
				ITrayIcon trayIcon;
				x.Value.TryGetTarget(out trayIcon);
				return trayIconViewModel == currentTrayIconViewModel && trayIcon != null;
			}).Select(x =>
			{
				ITrayIcon trayIcon;
				x.Value.TryGetTarget(out trayIcon);
				return trayIcon;
			}).FirstOrDefault();
			return result;
		}
	}
}