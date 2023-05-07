using Scaffold.Maui.Core;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Scaffold.Maui.Platforms.Android;

public partial class DisplayMenuItemslayer : IZBufferLayout
{
    private bool isBusy;
    private TaskCompletionSource<bool> tsc = new();

    public event VoidDelegate? DeatachLayer;

    public DisplayMenuItemslayer(View view)
	{
		InitializeComponent();
        Padding = view.GetContext()?.SafeArea ?? new Thickness();
        CommandSelectedMenu = new Command(ActionSelectedMenu);
        BindingContext = this;
        GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => Close().ConfigureAwait(false))
        });

        var obs = ScaffoldView.GetMenuItems(view).CollapsedItems;
        BindableLayout.SetItemsSource(stackMenu, obs);
	}

    public ICommand CommandSelectedMenu { get; private set; }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        tsc.TrySetResult(true);
    }

    private void ActionSelectedMenu(object param)
    {
        if (param is MenuItem menuItem)
        {
            menuItem.Command?.Execute(null);
        }
        Close().ConfigureAwait(false);
    }

    public async Task Show()
    {
        isBusy = true;
        Opacity = 0;
        await tsc.Task;
        await this.FadeTo(1, 180);
        isBusy = false;
    }

    public async Task Close()
    {
        if (isBusy)
            return;

        isBusy = true;

        await this.FadeTo(0, 180);
        DeatachLayer?.Invoke();
    }
}