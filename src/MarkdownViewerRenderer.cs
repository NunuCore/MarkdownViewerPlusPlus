using System;
using System.IO;
using Svg;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using static com.insanitydesign.MarkdownViewerPlusPlus.MarkdownViewer;

namespace com.insanitydesign.MarkdownViewerPlusPlus.Forms
{
    public class MarkdownViewerRenderer : AbstractRenderer
    {
        public MarkdownViewerHtmlPanel markdownViewerHtmlPanel;

        public MarkdownViewerRenderer(MarkdownViewer markdownViewer) : base(markdownViewer)
        {
        }

        protected override void Init()
        {
            base.Init();
            this.markdownViewerHtmlPanel = new MarkdownViewerHtmlPanel();
            this.markdownViewerHtmlPanel.ImageLoad += OnImageLoad;
            this.Controls.Add(this.markdownViewerHtmlPanel);
            this.Controls.SetChildIndex(this.markdownViewerHtmlPanel, 0);
        }

        public override void Render(string text, FileInformation fileInfo)
        {
            base.Render(text, fileInfo);
            this.markdownViewerHtmlPanel.Text = BuildHtml(ConvertedText, fileInfo.FileName);
        }

        public override void ScrollByRatioVertically(double scrollRatio)
        {
            this.markdownViewerHtmlPanel.ScrollByRatioVertically(scrollRatio);
        }

        protected void OnImageLoad(object sender, HtmlImageLoadEventArgs imageLoadEvent)
        {
            try
            {
                string src = imageLoadEvent.Src;

                if (string.IsNullOrWhiteSpace(src))
                {
                    return;
                }

                Uri uri;

                if (Path.IsPathRooted(src) || !Uri.TryCreate(src, UriKind.Absolute, out uri))
                {
                    imageLoadEvent.Handled = true;
                    ThreadPool.QueueUserWorkItem(state => LoadImageFromFile(src, imageLoadEvent));
                    return;
                }

                string extension = Path.GetExtension(uri.AbsolutePath);

                switch (uri.Scheme.ToLowerInvariant())
                {
                    case "file":
                        imageLoadEvent.Handled = true;
                        ThreadPool.QueueUserWorkItem(state => LoadImageFromFile(src, imageLoadEvent));
                        break;

                    case "http":
                    case "https":
                        if ((extension != null && extension.Equals(".svg", StringComparison.OrdinalIgnoreCase))
                            || uri.ToString().Contains("svg="))
                        {
                            using (WebClient webClient = new WebClient())
                            {
                                imageLoadEvent.Handled = true;
                                webClient.DownloadDataCompleted += (downloadSender, downloadEvent) =>
                                {
                                    OnDownloadDataCompleted(downloadEvent, imageLoadEvent);
                                };
                                webClient.DownloadDataAsync(uri);
                            }
                        }
                        break;
                }
            }
            catch
            {
            }
        }

        protected void LoadImageFromFile(string src, HtmlImageLoadEventArgs imageLoadEvent)
        {
            try
            {
                string localPath;
                Uri uri;

                if (Uri.TryCreate(src, UriKind.Absolute, out uri) && uri.IsFile)
                {
                    localPath = uri.LocalPath;
                }
                else
                {
                    localPath = src.Replace('/', Path.DirectorySeparatorChar);

                    if (!Path.IsPathRooted(localPath))
                    {
                        localPath = Path.GetFullPath(
                            Path.Combine(this.FileInfo.FileDirectory, localPath)
                        );
                    }
                }

                string extension = Path.GetExtension(localPath);

                if (extension != null && extension.Equals(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    ConvertSvgToBitmap(SvgDocument.Open<SvgDocument>(localPath), imageLoadEvent);
                }
                else
                {
                    imageLoadEvent.Callback((Bitmap)Image.FromFile(localPath, true));
                }
            }
            catch
            {
            }
        }

        protected Bitmap ConvertSvgToBitmap(SvgDocument svgDocument, HtmlImageLoadEventArgs imageLoadEvent)
        {
            Bitmap svgImage = new Bitmap(
                (int)svgDocument.Width,
                (int)svgDocument.Height,
                PixelFormat.Format32bppArgb
            );

            svgDocument.Draw(svgImage);
            imageLoadEvent.Callback(svgImage);
            imageLoadEvent.Handled = true;
            return svgImage;
        }

        protected void OnDownloadDataCompleted(
            DownloadDataCompletedEventArgs downloadEvent,
            HtmlImageLoadEventArgs imageLoadEvent
        )
        {
            using (MemoryStream stream = new MemoryStream(downloadEvent.Result))
            {
                ConvertSvgToBitmap(SvgDocument.Open<SvgDocument>(stream), imageLoadEvent);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.markdownViewerHtmlPanel != null)
            {
                this.markdownViewerHtmlPanel.ImageLoad -= OnImageLoad;
            }

            base.Dispose(disposing);
        }
    }
}
