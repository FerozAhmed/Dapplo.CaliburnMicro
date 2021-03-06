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
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dapplo.CaliburnMicro.Wizard.ViewModels;

#endregion

namespace Dapplo.CaliburnMicro.Wizard.Converters
{
	/// <summary>
	///     This converter is specially written for the WizardProgress
	/// </summary>
	public class IsProgressedConverter : IMultiValueConverter
	{
		object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if ((values[0] is ContentPresenter &&
				values[1] is int) == false)
			{
				return Visibility.Collapsed;
			}
			var checkNextItem = Convert.ToBoolean(parameter.ToString());
			var contentPresenter = values[0] as ContentPresenter;
			var progress = (int) values[1];
			var itemsControl = ItemsControl.ItemsControlFromItemContainer(contentPresenter);

			var index = itemsControl.ItemContainerGenerator.IndexFromContainer(contentPresenter);
			if (checkNextItem)
			{
				index++;
			}

			var wizardProgressViewModel = itemsControl.DataContext as WizardProgressViewModel;
			if (wizardProgressViewModel != null)
			{
				var percent = (int) ((double) index/wizardProgressViewModel.Wizard.WizardScreens.Count(x => x.IsVisible)*100);
				if (percent < progress)
				{
					return Visibility.Visible;
				}
			}
			return Visibility.Collapsed;
		}

		object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}