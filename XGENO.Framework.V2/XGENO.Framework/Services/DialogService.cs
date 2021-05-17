using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace XGENO.Framework.Services
{
    public class DialogService
    {
        bool _open = false;

        public DialogService()
        {
            // not thread safe
        }
        
        public async Task ShowAsync(string content, string title = default(string))
        {
            while (_open) 
            { 
                await Task.Delay(100); 
            }
            
            _open = true;

            var dialog = (title == null) ? new ContentDialog { Content = content, PrimaryButtonText = "OK" } : new ContentDialog { Content = content, Title = title, PrimaryButtonText = "OK" };

            await dialog.ShowAsync();

            _open = false;
        }

        public async Task ShowAsync(string content, string title, params UICommand[] commands)
        {
            while (_open)
            {
                await Task.Delay(100);
            }

            _open = true;

            var dialog = (title == null) ? new MessageDialog(content) : new MessageDialog(content, title);

            if (commands != null && commands.Any())
            {
                foreach (var item in commands)
                    dialog.Commands.Add(item);

                dialog.DefaultCommandIndex = 1;
            }

            await dialog.ShowAsync();

            _open = false;
        }
    }
}
