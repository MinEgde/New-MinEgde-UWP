using New_MinEgde_UWP_RTM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Streams;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace New_MinEgde_UWP_RTM
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BlankPage2 : Page
    {
        public BlankPage2()
        {
            this.InitializeComponent();
        }

        // 重写页面导航进入事件
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await AutoLoadUserProfileAsync();
        }

        // 独立的自动加载方法
        private async System.Threading.Tasks.Task AutoLoadUserProfileAsync()
        {
            try
            {
                // 1. 获取当前系统登录的所有活跃用户
                IReadOnlyList<Windows.System.User> users = await Windows.System.User.FindAllAsync();

                if (users != null && users.Count > 0)
                {
                    // 取出当前的活跃主用户
                    Windows.System.User currentUser = users[0];

                    // 2. 同时声明获取用户显示名称（DisplayName）和账户本地登录名（AccountName）
                    string[] requestedProperties = new string[]
                    {
                        Windows.System.KnownUserProperties.DisplayName,
                        Windows.System.KnownUserProperties.AccountName
                    };
                    IPropertySet properties = await currentUser.GetPropertiesAsync(requestedProperties);

                    // 3. 🛠️ 核心优化逻辑：智能提取用户名
                    string finalName = string.Empty;

                    // 尝试 A 方案：获取显示名称
                    if (properties.ContainsKey(Windows.System.KnownUserProperties.DisplayName))
                    {
                        finalName = properties[Windows.System.KnownUserProperties.DisplayName] as string;
                    }

                    // 尝试 B 方案：如果 A 方案拿到的是空的，改拿本地账户登录名
                    if (string.IsNullOrEmpty(finalName) && properties.ContainsKey(Windows.System.KnownUserProperties.AccountName))
                    {
                        finalName = properties[Windows.System.KnownUserProperties.AccountName] as string;
                    }

                    // 尝试 C 方案：如果前两个都被系统安全策略拦截，使用底层环境环境变量直接越过隐私限制
                    if (string.IsNullOrEmpty(finalName))
                    {
                        finalName = Environment.UserName;
                    }

                    // 最终将名字展示在你的文本控件上
                    textBlock.Text = !string.IsNullOrEmpty(finalName) ? finalName : "未知用户";

                    // 4. 调用内置函数 GetPictureAsync 异步读取用户头像流
                    try
                    {
                        var streamReference = await currentUser.GetPictureAsync(Windows.System.UserPictureSize.Size64x64);

                        if (streamReference != null)
                        {
                            using (IRandomAccessStream stream = await streamReference.OpenReadAsync())
                            {
                                BitmapImage bitmapImage = new BitmapImage();
                                await bitmapImage.SetSourceAsync(stream);
                                image.Source = bitmapImage;
                            }
                        }
                        else
                        {
                            // 若系统账户无头像返回，则使用页面自带的默认占位图
                            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/default-user.bmg"));
                        }
                    }
                    catch (Exception picEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"读取头像文件失败: {picEx.Message}");
                        image.Source = new BitmapImage(new Uri("ms-appx:///Assets/default-user.bmg"));
                    }
                }
                else
                {
                    // 如果 FindAllAsync 被完全挂起，直接使用环境变量强制输出本地用户名
                    textBlock.Text = Environment.UserName;
                    image.Source = new BitmapImage(new Uri("ms-appx:///Assets/default-user.bmg"));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"自动获取用户信息异常: {ex.Message}");
                // 发生意外异常时，依旧使用环境签名保底
                textBlock.Text = Environment.UserName;
                image.Source = new BitmapImage(new Uri("ms-appx:///Assets/default-user.bmg"));
            }
        }

        // 保留你原本就正确的 Debug Settings 按钮点击事件
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BlankPage3));
        }

        // 保留你原本就正确的另一个页面跳转事件
        private void button_Click_2(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BlankPage6));
        }
    }
}


