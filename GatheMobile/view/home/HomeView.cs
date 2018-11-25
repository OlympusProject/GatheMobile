using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace GatheMobile
{
	public class HomeView
  {
    String _id;
		EventHandler eventHandler;
		double lastScrollY = 0;
		static int height = (int)GlobalFunc.ConvertHeight(470);
		bool IsFetching = false;

		HomeViewModel ViewModel
    {
      get { return BindingContext as HomeViewModel; }
    }

    public HomeView()
    {
			BindingContext = new HomeViewModel();
			BackgroundColor = Color.Transparent;
  }
}
