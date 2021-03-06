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
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Dapplo.CaliburnMicro.Metro
{
	/// <summary>
	///     This (slightly modified) comes from
	///     <a href="https://github.com/ziyasal/Caliburn.Metro/blob/master/Caliburn.Metro.Core/MetroWindowManager.cs">here</a>
	///     and providers a Caliburn.Micro IWindowManager implementation. The Dapplo.CaliburnMicro.CaliburnMicroBootstrapper
	///     will
	///     take care of taking this (if available) and the MetroWindowManager will take care of instanciating a MetroWindow.
	///     Note: Currently there is no support for the DialogCoordinator yet..
	///     For more information see <a href="https://gist.github.com/ButchersBoy/4a7272f3ac104c5b1a54">here</a> and
	///     <a href="https://dragablz.net/2015/05/29/using-mahapps-dialog-boxes-in-a-mvvm-setup/">here</a>
	/// </summary>
	[Export(typeof(IWindowManager))]
	public class MetroWindowManager : WindowManager
	{
		private static readonly string[] Styles =
		{
			"Colors", "Fonts", "Controls", "Controls.AnimatedSingleRowTabControl", "Accents/Blue",
			"Accents/BaseLight"
		};

		/// <summary>
		///     Add all the resources to the Application
		/// </summary>
		public MetroWindowManager()
		{
			foreach (var style in Styles)
			{
				AddMahappsStyle(style);
			}
		}

		/// <summary>
		///     Export the IDialogCoordinator of MahApps, so ViewModels can open MahApps dialogs
		/// </summary>
		[Export]
		public IDialogCoordinator MahAppsDialogCoordinator => DialogCoordinator.Instance;

		/// <summary>
		///     Implement this to make specific configuration changes to your window.
		/// </summary>
		public Action<MetroWindow> OnConfigureWindow { get; set; }

		/// <summary>
		///     Implement this to make specific configuration changes to your owned (dialog) window.
		/// </summary>
		public Action<MetroWindow> OnConfigureOwnedWindow { get; set; }

		/// <summary>
		///     Add a ResourceDictionary for the specified MahApps style
		///     The Uri to the source is created by CreateMahappStyleUri
		/// </summary>
		/// <param name="style">
		///     Style name, this is actually what is added behind
		///     pack://application:,,,/MahApps.Metro;component/Styles/ (and .xaml is added)
		/// </param>
		public void AddMahappsStyle(string style)
		{
			AddResourceDictionary(CreateMahappStyleUri(style));
		}

		/// <summary>
		///     Add a single ResourceDictionary for the supplied source
		///     An example would be /Resources/Icons.xaml (which is in MahApps.Metro.Resources)
		/// </summary>
		/// <param name="source">Uri, e.g. /Resources/Icons.xaml or </param>
		public void AddResourceDictionary(Uri source)
		{
			var resourceDictionary = new ResourceDictionary
			{
				Source = source
			};
			Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
		}

		/// <summary>
		///     Remove all ResourceDictionaries for the specified MahApps style
		///     The Uri to the source is created by CreateMahappStyleUri
		/// </summary>
		/// <param name="style">string</param>
		public void RemoveMahappsStyle(string style)
		{
			var mahappsStyleUri = CreateMahappStyleUri(style);
			foreach (var resourceDirectory in Application.Current.Resources.MergedDictionaries.ToList())
			{
				if (resourceDirectory.Source == mahappsStyleUri)
				{
					Application.Current.Resources.MergedDictionaries.Remove(resourceDirectory);
				}
			}
		}

		/// <summary>
		///     Create a MetroWindow
		/// </summary>
		/// <param name="view"></param>
		/// <param name="windowIsView"></param>
		/// <returns></returns>
		public virtual MetroWindow CreateCustomWindow(object view, bool windowIsView)
		{
			MetroWindow result;
			if (windowIsView)
			{
				result = view as MetroWindow;
			}
			else
			{
				result = new MetroWindow
				{
					Content = view,
					SizeToContent = SizeToContent.WidthAndHeight
				};
			}
			result?.SetResourceReference(Control.BorderBrushProperty, "AccentColorBrush");
			result?.SetValue(Control.BorderThicknessProperty, new Thickness(1));

			return result;
		}

		/// <summary>Makes sure the view is a window or is wrapped by one.</summary>
		/// <param name="model">The view model.</param>
		/// <param name="view">The view.</param>
		/// <param name="isDialog">Whethor or not the window is being shown as a dialog.</param>
		/// <returns>The window.</returns>
		protected override Window EnsureWindow(object model, object view, bool isDialog)
		{
			MetroWindow window = null;
			Window inferOwnerOf;
			if (view is MetroWindow)
			{
				window = CreateCustomWindow(view, true);
				inferOwnerOf = InferOwnerOf(window);
				if (inferOwnerOf != null && isDialog)
				{
					window.Owner = inferOwnerOf;
				}
			}

			if (window == null)
			{
				window = CreateCustomWindow(view, false);
			}

			// Allow dialogs
			window.SetValue(DialogParticipation.RegisterProperty, model);
			window.SetValue(View.IsGeneratedProperty, true);
			inferOwnerOf = InferOwnerOf(window);
			if (inferOwnerOf != null)
			{
				// "Dialog", center it on top of the owner
				window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				window.Owner = inferOwnerOf;
				OnConfigureOwnedWindow?.Invoke(window);
			}
			else
			{
				// Free window, without owner
				window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				OnConfigureWindow?.Invoke(window);
			}
			// Just in case, make sure it's activated
			window.Activate();
			return window;
		}

		/// <summary>
		///     Create a MapApps Uri for the supplied style
		/// </summary>
		/// <param name="style">e.g. Fonts or Controls</param>
		/// <returns></returns>
		public static Uri CreateMahappStyleUri(string style)
		{
			return new Uri($"pack://application:,,,/MahApps.Metro;component/Styles/{style}.xaml", UriKind.RelativeOrAbsolute);
		}
	}
}