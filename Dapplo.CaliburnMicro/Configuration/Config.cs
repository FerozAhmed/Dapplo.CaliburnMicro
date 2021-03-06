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
using Dapplo.CaliburnMicro.Tree;
using Dapplo.InterfaceImpl.Extensions;
using Dapplo.Log.Facade;

#endregion

namespace Dapplo.CaliburnMicro.Configuration
{
	/// <summary>
	///     This implements a Caliburn-Micro Config UI
	/// </summary>
	public abstract class Config<TConfigScreen> : Conductor<TConfigScreen>.Collection.OneActive, IConfig<TConfigScreen>
		where TConfigScreen : class, IConfigScreen, ITreeNode<TConfigScreen>
	{
		// ReSharper disable once StaticMemberInGenericType
		private static readonly LogSource Log = new LogSource();
		private string _filter;
		private readonly ISet<ITransactionalProperties> _transactionalPropertyRegistrations = new HashSet<ITransactionalProperties>();

		/// <summary>
		///     Set a filter to apply to every IConfigScreen
		/// </summary>
		public virtual string Filter
		{
			get { return _filter; }
			set
			{
				_filter = value;
				NotifyOfPropertyChange(_filter);

				TreeItems.Clear();
				// Rebuild a tree for the ConfigScreens
				foreach (var configScreen in ConfigScreens.CreateTree(screen => screen.Contains(_filter)))
				{
					TreeItems.Add(configScreen);
				}
			}
		}

		/// <summary>
		///     This is called when the config needs to initialize stuff, it will call Initialize on every screen
		/// </summary>
		public virtual void Initialize()
		{
			Log.Verbose().WriteLine("Initializing config");

			foreach (var configScreen in ConfigScreens)
			{
				configScreen.Initialize(this);
			}
			// call StartTransaction on every registered ITransactionalProperties
			foreach (var transactionalProperties in _transactionalPropertyRegistrations)
			{
				transactionalProperties.StartTransaction();
			}
		}

		/// <summary>
		///     This is called when the config needs to cleanup things, it will call Terminate on every screen
		/// </summary>
		public virtual void Terminate()
		{
			Log.Verbose().WriteLine("Terminating config");
			foreach (var configScreen in ConfigScreens)
			{
				configScreen.Terminate();
			}
			// call RollbackTransaction on every registered ITransactionalProperties
			// this doesn't do anything if a after a CommitTransaction so is safe to do when terminating
			foreach (var transactionalProperties in _transactionalPropertyRegistrations)
			{
				transactionalProperties.RollbackTransaction();
			}
			_transactionalPropertyRegistrations.Clear();
		}

		/// <summary>
		///     The TConfigScreen items of the config
		/// </summary>
		public virtual IEnumerable<TConfigScreen> ConfigScreens { get; set; }

		/// <summary>
		///     This implements IConfig.ConfigScreens via ConfigScreens
		/// </summary>
		IEnumerable<IConfigScreen> IConfig.ConfigScreens
		{
			get { return ConfigScreens; }
			set { ConfigScreens = value as ICollection<TConfigScreen>; }
		}

		/// <summary>
		///     This implements IConfig.TreeItems via ConfigScreens
		/// </summary>
		public virtual ICollection<ITreeNode<TConfigScreen>> TreeItems { get; } = new ObservableCollection<ITreeNode<TConfigScreen>>();

		/// <summary>
		///     This return or sets the current config screen
		/// </summary>
		public virtual TConfigScreen CurrentConfigScreen
		{
			get { return ActiveItem; }
			set { ActivateItem(value); }
		}

		/// <summary>
		///     This implements IConfig.CurrentConfigScreen via CurrentConfigScreen
		/// </summary>
		IConfigScreen IConfig.CurrentConfigScreen
		{
			get { return CurrentConfigScreen; }
			set { CurrentConfigScreen = value as TConfigScreen; }
		}

		/// <summary>
		///     If CanCancel is true, this will call Rollback on all IConfigScreens and TryClose afterwards
		/// </summary>
		public virtual void Cancel()
		{
			if (CanCancel)
			{
				// Tell all IConfigScreen to Rollback
				foreach (var configScreen in ConfigScreens)
				{
					configScreen.Rollback();
				}

				TryClose(false);
			}
		}

		/// <summary>
		/// Implement IConfig.TreeItems via the generic version
		/// </summary>
		// ReSharper disable once SuspiciousTypeConversion.Global
		ICollection<ITreeNode<IConfigScreen>> IConfig.TreeItems => TreeItems as ICollection<ITreeNode<IConfigScreen>>;

		/// <summary>
		///     check every IConfigScreen if it can close
		/// </summary>
		public virtual bool CanCancel
		{
			get
			{
				var result = true;
				CanClose(b => result = b);
				return result;
			}
		}

		/// <summary>
		///     If CanOk is true, this will call Commit on all IConfigScreens and TryClose afterwards
		/// </summary>
		public virtual void Ok()
		{
			if (CanOk)
			{
				// Tell all IConfigScreen to commit
				foreach (var configScreen in ConfigScreens)
				{
					configScreen.Commit();
				}
				// call CommitTransaction on every registered ITransactionalProperties
				foreach (var transactionalProperties in _transactionalPropertyRegistrations)
				{
					transactionalProperties.CommitTransaction();
				}
				TryClose(true);
			}
		}

		/// <summary>
		/// Register an instanceof ITransactionalProperties to be included in the transaction (rollback or commit will be called for you)
		/// </summary>
		/// <param name="transactionalProperties"></param>
		public void Register(ITransactionalProperties transactionalProperties)
		{
			_transactionalPropertyRegistrations.Add(transactionalProperties);
		}

		/// <summary>
		///     check every IConfigScreen if it can close
		/// </summary>
		public virtual bool CanOk
		{
			get
			{
				var result = true;
				CanClose(b => result = b);
				return result;
			}
		}

		/// <summary>
		///     Called to check whether or not this instance can close.
		/// </summary>
		/// <param name="callback">The implementor calls this action with the result of the close check.</param>
		public override void CanClose(Action<bool> callback)
		{
			var result = true;
			foreach (var configScreen in ConfigScreens)
			{
				configScreen.CanClose(canClose => result &= canClose);
			}
			callback(result);
		}

		/// <summary>
		///     Called when deactivating, this will terminate the configuration
		///     Makes sure the items are cleared again, otherwise we have everything double
		/// </summary>
		/// <param name="close">Inidicates whether this instance will be closed.</param>
		protected override void OnDeactivate(bool close)
		{
			Terminate();
			base.OnDeactivate(close);
			Items.Clear();
			TreeItems.Clear();
		}

		/// <summary>
		///     Called when activating, will build a tree of the config screens
		/// </summary>
		protected override void OnActivate()
		{
			Items.AddRange(ConfigScreens);

			Initialize();

			// Build a tree for the ConfigScreens
			foreach (var configScreen in ConfigScreens.CreateTree())
			{
				TreeItems.Add(configScreen);
			}

			base.OnActivate();
		}

		/// <summary>
		///     Activates the specified config screen, and sends notify property changed events.
		/// </summary>
		/// <param name="configScreen">The TConfigScreen to activate.</param>
		public override void ActivateItem(TConfigScreen configScreen)
		{
			if (configScreen == null || configScreen.CanActivate)
			{
				base.ActivateItem(configScreen);
				NotifyOfPropertyChange(nameof(CurrentConfigScreen));
				NotifyOfPropertyChange(nameof(CanCancel));
				NotifyOfPropertyChange(nameof(CanOk));
			}
		}
	}
}