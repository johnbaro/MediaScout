using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MediaScoutGUI.Controls
{
	public class AnimatedImage : Image
	{
		public static readonly DependencyProperty FrameIndexProperty;

		public new static readonly DependencyProperty SourceProperty;

		public static readonly DependencyProperty AnimationRepeatBehaviorProperty;

		public static readonly DependencyProperty UriSourceProperty;

		public int FrameIndex
		{
			get
			{
				return (int)base.GetValue(AnimatedImage.FrameIndexProperty);
			}
			set
			{
				base.SetValue(AnimatedImage.FrameIndexProperty, value);
			}
		}

		public List<BitmapFrame> Frames
		{
			get;
			private set;
		}

		public RepeatBehavior AnimationRepeatBehavior
		{
			get
			{
				return (RepeatBehavior)base.GetValue(AnimatedImage.AnimationRepeatBehaviorProperty);
			}
			set
			{
				base.SetValue(AnimatedImage.AnimationRepeatBehaviorProperty, value);
			}
		}

		/*public new BitmapImage Source
		{
			get
			{
				return (BitmapImage)base.GetValue(AnimatedImage.SourceProperty);
			}
			set
			{
				base.SetValue(AnimatedImage.SourceProperty, value);
			}
		}*/

		public Uri UriSource
		{
			get
			{
				return (Uri)base.GetValue(AnimatedImage.UriSourceProperty);
			}
			set
			{
				base.SetValue(AnimatedImage.UriSourceProperty, value);
			}
		}

		private Int32Animation Animation
		{
			get;
			set;
		}

		private bool IsAnimationWorking
		{
			get;
			set;
		}

		static AnimatedImage()
		{
			AnimatedImage.FrameIndexProperty = DependencyProperty.Register("FrameIndex", typeof(int), typeof(AnimatedImage), new UIPropertyMetadata(0, new PropertyChangedCallback(AnimatedImage.ChangingFrameIndex)));
			AnimatedImage.SourceProperty = DependencyProperty.Register("Source", typeof(BitmapImage), typeof(AnimatedImage), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(AnimatedImage.OnSourceChanged)));
			AnimatedImage.AnimationRepeatBehaviorProperty = DependencyProperty.Register("AnimationRepeatBehavior", typeof(RepeatBehavior), typeof(AnimatedImage), new PropertyMetadata(null));
			AnimatedImage.UriSourceProperty = DependencyProperty.Register("UriSource", typeof(Uri), typeof(AnimatedImage), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(AnimatedImage.OnSourceChanged)));
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedImage), new FrameworkPropertyMetadata(typeof(AnimatedImage)));
		}

		protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs e)
		{
			this.ClearAnimation();
			BitmapImage bitmapImage;
			if (e.NewValue is string)
			{
				bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.UriSource = new Uri(e.NewValue as string);
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.EndInit();
			}
			else
			{
				if (!(e.NewValue is BitmapImage))
				{
					if (e.NewValue == null)
					{
						base.Source = null;
					}
					return;
				}
				bitmapImage = (e.NewValue as BitmapImage);
			}
			BitmapDecoder bitmapDecoder;
			if (bitmapImage.StreamSource != null)
			{
				bitmapDecoder = BitmapDecoder.Create(bitmapImage.StreamSource, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
			}
			else
			{
				if (!(bitmapImage.UriSource != null))
				{
					return;
				}
				bitmapDecoder = BitmapDecoder.Create(bitmapImage.UriSource, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
			}
			if (bitmapDecoder.Frames.Count == 1)
			{
				base.Source = bitmapDecoder.Frames[0];
				return;
			}
			this.Frames = bitmapDecoder.Frames.ToList<BitmapFrame>();
			this.PrepareAnimation();
		}

		private void ClearAnimation()
		{
			if (this.Animation != null)
			{
				base.BeginAnimation(AnimatedImage.FrameIndexProperty, null);
			}
			this.IsAnimationWorking = false;
			this.Animation = null;
			this.Frames = null;
		}

		private void PrepareAnimation()
		{
			this.Animation = new Int32Animation(0, this.Frames.Count - 1, new Duration(new TimeSpan(0, 0, 0, this.Frames.Count / 10, (int)(((double)this.Frames.Count / 10.0 - (double)(this.Frames.Count / 10)) * 1000.0))))
			{
				RepeatBehavior = RepeatBehavior.Forever
			};
			base.Source = this.Frames[0];
			base.BeginAnimation(AnimatedImage.FrameIndexProperty, this.Animation);
			this.IsAnimationWorking = true;
		}

		private static void ChangingFrameIndex(DependencyObject dp, DependencyPropertyChangedEventArgs e)
		{
			AnimatedImage animatedImage = dp as AnimatedImage;
			if (animatedImage == null || !animatedImage.IsAnimationWorking)
			{
				return;
			}
			int index = (int)e.NewValue;
			animatedImage.Source = animatedImage.Frames[index];
			animatedImage.InvalidateVisual();
		}

		private static void OnSourceChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
		{
			((AnimatedImage)dp).OnSourceChanged(e);
		}
	}
}
