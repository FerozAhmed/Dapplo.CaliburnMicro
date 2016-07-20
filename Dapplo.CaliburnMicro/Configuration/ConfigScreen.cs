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
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Dapplo.CaliburnMicro.Misc;

#endregion

namespace Dapplo.CaliburnMicro.Configuration
{
	/// <summary>
	///     A basic implementation of IConfigScreen
	/// </summary>
	public abstract class ConfigScreen : Screen, IConfigScreen
	{
		private bool _isEnabled = true;
		private bool _canActivate = true;
		private bool _isVisible = true;

		#region ITreeNode
		/// <summary>
		/// Used to showing this inside a tree
		/// </summary>
		public virtual ITreeNode<IConfigScreen> ParentNode { get; set; }

		/// <summary>
		/// Used to showing this inside a tree
		/// </summary>
		public virtual ICollection<ITreeNode<IConfigScreen>> ChildNodes { get; set; } = new ObservableCollection<ITreeNode<IConfigScreen>>();
		#endregion

		/// <summary>
		///     The parent under which the IConfigScreen is shown, 0 is root
		/// </summary>
		public virtual int ParentId { get; }
		
		/// <summary>
		/// The Id of this config screen, is also used to order children of a parent
		/// </summary>
		public abstract int Id { get; }

		/// <summary>
		///     Do some general initialization, if needed
		///     This is called when the config UI is initializing
		/// </summary>
		public virtual void Initialize(IConfig config)
		{
		}

		/// <summary>
		///     Terminate the config screen.
		///     This is called when the parent config UI terminates, no matter if this config screen was shown
		/// </summary>
		public virtual void Terminate()
		{
		}

		/// <summary>
		///     Returns if the config screen can be activated
		/// </summary>
		public virtual bool CanActivate
		{
			get { return _canActivate; }
			protected set
			{
				_canActivate = value;
				NotifyOfPropertyChange(nameof(CanActivate));
			}
		}

		/// <summary>
		///     Returns if the config screen can be selected
		/// </summary>
		public virtual bool IsEnabled
		{
			get { return _isEnabled; }
			protected set
			{
				_isEnabled = value;
				NotifyOfPropertyChange(nameof(IsEnabled));
			}
		}

		/// <summary>
		///     Returns if the config screen is visible
		/// </summary>
		public virtual bool IsVisible
		{
			get { return _isVisible; }
			protected set
			{
				_isVisible = value;
				NotifyOfPropertyChange(nameof(IsVisible));
			}
		}
	}
}