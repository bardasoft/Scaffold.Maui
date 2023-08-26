using ScaffoldLib.Maui.Core;
using ScaffoldLib.Maui.Internal;
using ScaffoldLib.Maui.Toolkit.FlyoutViewPlatforms;
using System.Collections;

namespace ScaffoldLib.Maui.Containers.WinUI;

public partial class NavigationBar : INavigationBar, IWindowsBehavior
{
    private readonly View _view;
    private readonly IAgent _agent;
    private readonly IScaffold _context;
    private IBackButtonBehavior? backButtonBehavior;
    private Color? foregroundColor;
    private bool isVisibleButtonMoreMenu;

    public NavigationBar(View view, IAgent agent)
    {
        _view = view;
        _agent = agent;
        _context = agent.Context;

        InitializeComponent();

        buttonBack.TapCommand = new Command(() =>
        {
            agent.OnBackButton();
        });

        buttonMenu.TapCommand = new Command(() =>
        {
            agent.OnMenuButton();
        });
    }

    public Color? ForegroundColor
    {
        get => foregroundColor;
        set
        {
            foregroundColor = value;
            OnPropertyChanged(nameof(ForegroundColor));
        }
    }

    public bool IsVisibleButtonMoreMenu
    {
        get => isVisibleButtonMoreMenu;
        set
        {
            isVisibleButtonMoreMenu = value;
            OnPropertyChanged(nameof(IsVisibleButtonMoreMenu));
        }
    }

    public Rect[] UndragArea
    {
        get
        {
            var rects = new List<Rect>();

            if (buttonBack.IsVisible)
                rects.Add(buttonBack.AbsRect());

            if (buttonMenu.IsVisible)
                rects.Add(buttonMenu.AbsRect());

            foreach (View item in stackMenu)
                rects.Add(item.AbsRect());

            return rects.ToArray();
        }
    }

    public void UpdateTitle(string? title)
    {
        labelTitle.Text = title;
    }

    public void UpdateBackButtonBehavior(IBackButtonBehavior? behavior)
    {
        backButtonBehavior = behavior;
    }

    public void UpdateMenuItems(Core.MenuItemCollection menu)
    {
        BindableLayout.SetItemsSource(stackMenu, menu.VisibleItems);
        menu.CollapsedItems.CollectionChanged += CollapsedItems_CollectionChanged;
        IsVisibleButtonMoreMenu = menu.CollapsedItems.Count > 0;
    }

    private void CollapsedItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        IsVisibleButtonMoreMenu = ((IList)sender!).Count > 0;
    }

    public void UpdateNavigationBarBackgroundColor(Color color)
    {
        BackgroundColor = color;
    }

    public void UpdateNavigationBarForegroundColor(Color color)
    {
        labelTitle.TextColor = color;
        imageBackButton.TintColor = color;
        ForegroundColor = color;
    }

    public void UpdateNavigationBarVisible(bool visible)
    {
        IsVisible = visible;
    }

    public void UpdateSafeArea(Thickness safeArea)
    {
        HeightRequest = safeArea.Top;
    }

    public void UpdateBackButtonVisibility(bool isVisible)
    {
        buttonBack.IsVisible = isVisible;
    }
}