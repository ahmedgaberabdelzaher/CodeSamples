using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace HrApp.Controls
{
    public partial class MenuControl : ContentView
    {
        public MenuControl()
        {
            InitializeComponent();
        }
        public static readonly BindableProperty FirstItemCommandProperty =
BindableProperty.Create(
    propertyName: "FirstItemCommand",
    returnType: typeof(ICommand),
    declaringType: typeof(MenuControl),
    defaultValue: null,
    defaultBindingMode: BindingMode.OneWay,
    validateValue: null,
    propertyChanged: OnFirstItemTappedChanged
    );

        private static void OnFirstItemTappedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as MenuControl;

            if (control != null)
            {
                control.firstItem.GestureRecognizers.Clear();
                control.firstItem.GestureRecognizers.Add(
                    new TapGestureRecognizer()
                    {
                        Command = new Command((o) => {

                            var command = (ICommand)bindable.GetValue(FirstItemCommandProperty);

                            if (command != null && command.CanExecute(null))
                                command.Execute(null);
                        })
                    }
                );
            }
        }

        public ICommand FirstItemCommand
        {
            get { return (ICommand)GetValue(FirstItemCommandProperty); }
            set { SetValue(FirstItemCommandProperty, value); }
        }



        public static readonly BindableProperty FirstItemTextProperty = BindableProperty.Create(
                                                      propertyName: "FirstItemText",
                                                      returnType: typeof(string),
                                                      declaringType: typeof(MenuControl),
                                                      defaultValue: "",
                                                      defaultBindingMode: BindingMode.TwoWay,
                                                      propertyChanged: TitleTextPropertyChanged);

        public string FirstItemText
        {
            get { return base.GetValue(FirstItemTextProperty).ToString(); }
            set { base.SetValue(FirstItemTextProperty, value); }
        }

        private static void TitleTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MenuControl)bindable;
            control.firstItemText.Text = newValue.ToString();
        }

        public static readonly BindableProperty FirstItemImageProperty = BindableProperty.Create(
                                                        propertyName: "FirstItemImage",
                                                        returnType: typeof(string),
                                                        declaringType: typeof(MenuControl),
                                                        defaultValue: "",
                                                        defaultBindingMode: BindingMode.TwoWay,
                                                        propertyChanged: ImageSourcePropertyChanged);

        public string FirstItemImage
        {
            get { return base.GetValue(FirstItemImageProperty).ToString(); }
            set { base.SetValue(FirstItemImageProperty, value); }
        }

        private static void ImageSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MenuControl)bindable;
            control.firstItemImage.Source = ImageSource.FromFile(newValue.ToString());
        }

        #region second Item
        public static readonly BindableProperty SecondItemCommandProperty =
BindableProperty.Create(
  propertyName: "SecondItemCommand",
  returnType: typeof(ICommand),
  declaringType: typeof(MenuControl),
  defaultValue: null,
  defaultBindingMode: BindingMode.OneWay,
  validateValue: null,
  propertyChanged: OnSecondItemTappedChanged
  );

        private static void OnSecondItemTappedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as MenuControl;

            if (control != null)
            {
                control.secondItem.GestureRecognizers.Clear();
                control.secondItem.GestureRecognizers.Add(
                    new TapGestureRecognizer()
                    {
                        Command = new Command((o) => {

                            var command = (ICommand)bindable.GetValue(SecondItemCommandProperty);

                            if (command != null && command.CanExecute(null))
                                command.Execute(null);
                        })
                    }
                );
            }
        }

        public ICommand SecondItemCommand
        {
            get { return (ICommand)GetValue(SecondItemCommandProperty); }
            set { SetValue(SecondItemCommandProperty, value); }
        }



        public static readonly BindableProperty SecondItemTextProperty = BindableProperty.Create(
                                                      propertyName: "SecondItemText",
                                                      returnType: typeof(string),
                                                      declaringType: typeof(MenuControl),
                                                      defaultValue: "",
                                                      defaultBindingMode: BindingMode.TwoWay,
                                                      propertyChanged: SecondItemTexttPropertyChanged);

        private static void SecondItemTexttPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MenuControl)bindable;
            control.secondItemText.Text = newValue.ToString();
        }

        public string SecondItemText
        {
            get { return base.GetValue(SecondItemTextProperty).ToString(); }
            set { base.SetValue(SecondItemTextProperty, value); }
        }


        public static readonly BindableProperty SecondItemImageProperty = BindableProperty.Create(
                                                        propertyName: "SecondItemImage",
                                                        returnType: typeof(string),
                                                        declaringType: typeof(MenuControl),
                                                        defaultValue: "",
                                                        defaultBindingMode: BindingMode.TwoWay,
                                                        propertyChanged: SecondImageSourcePropertyChanged);

        private static void SecondImageSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MenuControl)bindable;
            control.secondItemImage.Source = ImageSource.FromFile(newValue.ToString());
        }

        public string SecondItemImage
        {
            get { return base.GetValue(SecondItemImageProperty).ToString(); }
            set { base.SetValue(SecondItemImageProperty, value); }
        }



        #endregion


        #region Third Item
        public static readonly BindableProperty ThirdItemCommandProperty =
BindableProperty.Create(
  propertyName: "ThirdItemCommand",
  returnType: typeof(ICommand),
  declaringType: typeof(MenuControl),
  defaultValue: null,
  defaultBindingMode: BindingMode.OneWay,
  validateValue: null,
  propertyChanged: OnThirdItemTappedChanged
  );

        private static void OnThirdItemTappedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as MenuControl;

            if (control != null)
            {
                control.ThirdItem.GestureRecognizers.Clear();
                control.ThirdItem.GestureRecognizers.Add(
                    new TapGestureRecognizer()
                    {
                        Command = new Command((o) => {

                            var command = (ICommand)bindable.GetValue(ThirdItemCommandProperty);

                            if (command != null && command.CanExecute(null))
                                command.Execute(null);
                        })
                    }
                );
            }
        }


        public ICommand ThirdItemCommand
        {
            get { return (ICommand)GetValue(ThirdItemCommandProperty); }
            set { SetValue(ThirdItemCommandProperty, value); }
        }



        public static readonly BindableProperty ThirdItemTitleTextProperty = BindableProperty.Create(
                                                      propertyName: "ThirdItemTitleText",
                                                      returnType: typeof(string),
                                                      declaringType: typeof(MenuControl),
                                                      defaultValue: "",
                                                      defaultBindingMode: BindingMode.TwoWay,
                                                      propertyChanged: ThirdItemTexttPropertyChanged);

        private static void ThirdItemTexttPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MenuControl)bindable;
            control.ThirdItemText.Text = newValue.ToString();
        }

    

        public string ThirdItemTitleText
        {
            get { return base.GetValue(ThirdItemTitleTextProperty).ToString(); }
            set { base.SetValue(ThirdItemTitleTextProperty, value); }
        }


        public static readonly BindableProperty ThirdItemImageSourceProperty = BindableProperty.Create(
                                                        propertyName: "ThirdItemImageSource",
                                                        returnType: typeof(string),
                                                        declaringType: typeof(MenuControl),
                                                        defaultValue: "",
                                                        defaultBindingMode: BindingMode.TwoWay,
                                                        propertyChanged: THirdImageSourcePropertyChanged);

        private static void THirdImageSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MenuControl)bindable;
            control.ThirdItemImage.Source = ImageSource.FromFile(newValue.ToString());
        }

    

        public string ThirdItemImageSource
        {
            get { return base.GetValue(ThirdItemImageSourceProperty).ToString(); }
            set { base.SetValue(ThirdItemImageSourceProperty, value); }
        }



        #endregion

        #region Fourth Item
        public static readonly BindableProperty FourthItemCommandProperty =
BindableProperty.Create(
  propertyName: "FourthItemCommand",
  returnType: typeof(ICommand),
  declaringType: typeof(MenuControl),
  defaultValue: null,
  defaultBindingMode: BindingMode.OneWay,
  validateValue: null,
  propertyChanged: OnFourthItemTappedChanged
  );

        private static void OnFourthItemTappedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as MenuControl;

            if (control != null)
            {
                control.fourthItem.GestureRecognizers.Clear();
                control.fourthItem.GestureRecognizers.Add(
                    new TapGestureRecognizer()
                    {
                        Command = new Command((o) => {

                            var command = (ICommand)bindable.GetValue(FourthItemCommandProperty);

                            if (command != null && command.CanExecute(null))
                                command.Execute(null);
                        })
                    }
                );
            }
        }



        public ICommand FourthItemCommand
        {
            get { return (ICommand)GetValue(FourthItemCommandProperty); }
            set { SetValue(FourthItemCommandProperty, value); }
        }



        public static readonly BindableProperty FourthItemTitleTextProperty = BindableProperty.Create(
                                                      propertyName: "FourthItemTitleText",
                                                      returnType: typeof(string),
                                                      declaringType: typeof(MenuControl),
                                                      defaultValue: "",
                                                      defaultBindingMode: BindingMode.TwoWay,
                                                      propertyChanged: FourthItemTexttPropertyChanged);

        private static void FourthItemTexttPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MenuControl)bindable;
            control.fourthItemText.Text = newValue.ToString();
        }

        public string FourthItemTitleText
        {
            get { return base.GetValue(FourthItemTitleTextProperty).ToString(); }
            set { base.SetValue(FourthItemTitleTextProperty, value); }
        }


        public static readonly BindableProperty FourthItemImageSourceProperty = BindableProperty.Create(
                                                        propertyName: "FourthItemImageSource",
                                                        returnType: typeof(string),
                                                        declaringType: typeof(MenuControl),
                                                        defaultValue: "",
                                                        defaultBindingMode: BindingMode.TwoWay,
                                                        propertyChanged: FourthImageSourcePropertyChanged);

        private static void FourthImageSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MenuControl)bindable;
            control.fourthItemImage.Source = ImageSource.FromFile(newValue.ToString());
        }

        



        public string FourthItemImageSource
        {
            get { return base.GetValue(FourthItemImageSourceProperty).ToString(); }
            set { base.SetValue(FourthItemImageSourceProperty, value); }
        }



        #endregion

        #region Fifth Item
        public static readonly BindableProperty FifthItemCommandProperty =
BindableProperty.Create(
  propertyName: "FifthItemCommand",
  returnType: typeof(ICommand),
  declaringType: typeof(MenuControl),
  defaultValue: null,
  defaultBindingMode: BindingMode.OneWay,
  validateValue: null,
  propertyChanged: OnFifthItemTappedChanged
  );

        private static void OnFifthItemTappedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as MenuControl;

            if (control != null)
            {
                control.fifthItem.GestureRecognizers.Clear();
                control.fifthItem.GestureRecognizers.Add(
                    new TapGestureRecognizer()
                    {
                        Command = new Command((o) => {

                            var command = (ICommand)bindable.GetValue(FifthItemCommandProperty);

                            if (command != null && command.CanExecute(null))
                                command.Execute(null);
                        })
                    }
                );
            }
        }




        public ICommand FifthItemCommand
        {
            get { return (ICommand)GetValue(FifthItemCommandProperty); }
            set { SetValue(FifthItemCommandProperty, value); }
        }



        public static readonly BindableProperty FifthItemTitleTextProperty = BindableProperty.Create(
                                                      propertyName: "FifthItemTitleText",
                                                      returnType: typeof(string),
                                                      declaringType: typeof(MenuControl),
                                                      defaultValue: "",
                                                      defaultBindingMode: BindingMode.TwoWay,
                                                      propertyChanged: FifthItemTexttPropertyChanged);

        private static void FifthItemTexttPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MenuControl)bindable;
            control.fifthItemText.Text = newValue.ToString();
        }

     
        public string FifthItemTitleText
        {
            get { return base.GetValue(FifthItemTitleTextProperty).ToString(); }
            set { base.SetValue(FifthItemTitleTextProperty, value); }
        }


        public static readonly BindableProperty FifthItemImageSourceProperty = BindableProperty.Create(
                                                        propertyName: "FifthItemImageSource",
                                                        returnType: typeof(string),
                                                        declaringType: typeof(MenuControl),
                                                        defaultValue: "",
                                                        defaultBindingMode: BindingMode.TwoWay,
                                                        propertyChanged: FifthImageSourcePropertyChanged);

        private static void FifthImageSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {

            var control = (MenuControl)bindable;
            control.fifthItemImage.Source = ImageSource.FromFile(newValue.ToString());

        }




        public string FifthItemImageSource
        {
            get { return base.GetValue(FifthItemImageSourceProperty).ToString(); }
            set { base.SetValue(FifthItemImageSourceProperty, value); }
        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {

        }


        #endregion
    }
}
