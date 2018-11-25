using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using XLabs.Forms.Mvvm;

namespace GatheMobile
{
  public class HomeViewModel : ViewModel
	{
    string _title;
    public string title { get { return _title.ToUpper(); } set { _title = value; } }
    public string id { get; set; }
    public Color background { get; set; }

    ObservableCollection<EventListModel> _event;
    public ObservableCollection<EventListModel> event
    {
      get
      {
        return _event;
      }
    	set
    	{
        SetProperty<ObservableCollection<EventListModel>>(ref _event, value);
    	}
    }

    ObservableCollection<CommunityListModel> _community;
    public ObservableCollection<CommunityListModel> community
    {
      get
      {
        return _community;
      }
    	set
    	{
        SetProperty<ObservableCollection<CommunityListModel>>(ref _community, value);
    	}
    }
  }
}
