﻿/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    Download.cs
 *  Version: 1.0.0.0
 *
 *  Copyright ©2011 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 *__________________________________________________________________________
 *
 *  Notes:
 *
 *  Represents end executes a download operation.
 *   
 ***************************************************************************/

#region Using
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;
#endregion

namespace TabbedWPFSample
{
    internal class Download : ViewModel
    {
        #region Fields
        private const string SHELL = "explorer.exe";
        private string file;
        WebClient client;

        DelegateCommand openCommand;
        DelegateCommand openFolderCommand;
        #endregion

        #region Ctors
        public Download( string url, string file )
        {
            if ( String.IsNullOrEmpty( url ) )
                throw new ArgumentNullException( "url" );

            if ( String.IsNullOrEmpty( file ) )
                throw new ArgumentNullException( "file" );

            this.URL = url;
            this.FileName = new FileInfo( file ).Name;
            this.file = file;

            openCommand = new DelegateCommand( OnOpenDownloadedFile, CanOpenDownloadedFile );
            this.Commands.Add( openCommand );
            openFolderCommand = new DelegateCommand( OnOpenDownloadedFileFolder, CanOpenDownloadedFile );
            this.Commands.Add( openFolderCommand );
        }
        #endregion


        #region Overrides
        public override int GetHashCode()
        {
            return String.Format( "{0}_{1}", _URL, file ).GetHashCode();
        }

        public override bool Equals( object obj )
        {
            if ( obj is Download )
                return this == (Download)obj;

            return false;
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            file = null;
            client = null;
            openCommand = null;
            openFolderCommand = null;
        }
        #endregion

        #region Methods
        public void Start()
        {
            if ( !IsDownloading )
                this.DownloadFile();
        }

        public void Cancel()
        {
            if ( IsDownloading && client.IsBusy )
                client.CancelAsync();
        }

        private void DownloadFile()
        {
            if ( client == null )
                client = new WebClient();

            DownloadProgressChangedEventHandler progressHandler = ( sender, e ) =>
            {
                this.Progress = e.ProgressPercentage;
                CommandManager.InvalidateRequerySuggested();
            };

            AsyncCompletedEventHandler completeHandler = null;
            completeHandler = ( sender, e ) =>
            {
                using ( client )
                {
                    client.DownloadProgressChanged -= progressHandler;
                    client.DownloadFileCompleted -= completeHandler;

                    this.IsCancelled = e.Cancelled;
                    this.IsDownloading = false;

                    if ( !e.Cancelled && File.Exists( file ) )
                    {
                        this.FileSize = new FileInfo( file ).GetFileSize();
                        this.IsDownloadComplete = true;
                    }

                    CommandManager.InvalidateRequerySuggested();
                }

                client = null;
            };

            client.DownloadProgressChanged += progressHandler;
            client.DownloadFileCompleted += completeHandler;

            try
            {
                client.DownloadFileAsync( new Uri( this.URL ), file );
            }
            catch ( Exception ex )
            {
                CommandManager.InvalidateRequerySuggested();
                MessageBox.Show( ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error );
                return;
            }

            this.FileSize = "N/A";
            this.IsCancelled = false;
            this.IsDownloadComplete = false;
            this.IsDownloading = true;

            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region Properties
        private string _URL;
        public string URL
        {
            get
            {
                return _URL;
            }
            protected set
            {
                if ( String.Compare( _URL, value, true ) == 0 )
                    return;

                _URL = value;
                RaisePropertyChanged( "URL" );
            }
        }


        private int _Progress;
        public int Progress
        {
            get
            {
                return _Progress;
            }
            protected set
            {
                if ( _Progress == value )
                    return;

                _Progress = value;
                RaisePropertyChanged( "Progress" );
            }
        }


        private bool _IsDownloading;
        public bool IsDownloading
        {
            get
            {
                return _IsDownloading;
            }
            protected set
            {
                if ( _IsDownloading == value )
                    return;

                _IsDownloading = value;
                RaisePropertyChanged( "IsDownloading" );
            }
        }


        private bool _IsCancelled;
        public bool IsCancelled
        {
            get
            {
                return _IsCancelled;
            }
            protected set
            {
                if ( _IsCancelled == value )
                    return;

                _IsCancelled = value;
                RaisePropertyChanged( "IsCancelled" );
            }
        }


        private string _FileName;
        public string FileName
        {
            get
            {
                return _FileName;
            }
            protected set
            {
                if ( String.Compare( _FileName, value, true ) == 0 )
                    return;

                _FileName = value;
                RaisePropertyChanged( "FileName" );
            }
        }

        private string _FileSize;
        public string FileSize
        {
            get
            {
                return _FileSize;
            }
            protected set
            {
                if ( String.Compare( _FileSize, value, true ) == 0 )
                    return;

                _FileSize = value;
                RaisePropertyChanged( "FileSize" );
            }
        }

        private bool _IsDownloadComplete;
        public bool IsDownloadComplete
        {
            get
            {
                return _IsDownloadComplete;
            }
            protected set
            {
                if ( _IsDownloadComplete == value )
                    return;

                _IsDownloadComplete = value;
                RaisePropertyChanged( "IsDownloadComplete" );
            }
        }

        public ICommand OpenDownloadedFile
        {
            get
            {
                return openCommand;
            }
        }

        public ICommand OpenDownloadedFileFolder
        {
            get
            {
                return openFolderCommand;
            }
        }
        #endregion

        #region Operators
        public static bool operator ==( Download d1, Download d2 )
        {
            if ( Object.ReferenceEquals( d1, null ) )
                return Object.ReferenceEquals( d2, null );

            if ( Object.ReferenceEquals( d2, null ) )
                return Object.ReferenceEquals( d1, null );

            return String.Compare( d1.URL, d2.URL, true ) == 0 &&
                String.Compare( d1.file, d2.file, true ) == 0;
        }

        public static bool operator !=( Download d1, Download d2 )
        {
            return !( d1 == d2 );
        }
        #endregion

        #region Event Handlers
        private void OnOpenDownloadedFile()
        {
            if ( IsDownloadComplete && File.Exists( file ) )
            {
                try
                {
                    Process.Start( file );
                }
                catch { }
            }
        }

        private void OnOpenDownloadedFileFolder()
        {
            if ( IsDownloadComplete && File.Exists( file ) )
            {
                try
                {
                    Process.Start( SHELL, String.Format( @"/select, ""{0}""", file ) );
                }
                catch { }
            }
        }

        private bool CanOpenDownloadedFile()
        {
            return !IsCancelled;
        }
        #endregion
    }
}
