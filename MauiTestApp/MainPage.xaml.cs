using System.Net;
using System.Runtime.CompilerServices;

namespace MauiTestApp;

public partial class MainPage : ContentPage
{
 #region Construction
 /////////////////////////////////////////////////////////////////////////////////////////////

 public MainPage()
 {
  InitializeComponent();
  IncrementClickCount = new Command(OnIncrementClickCount, ValidateIncrementClickCount);
  EnableIncrementClickCount = new Command(OnEnableIncrementClickCount);
  DisableIncrementClickCount = new Command(OnDisableIncrementClickCount);
  Download = new Command(OnDownload);

  BindingContext = this;
 }

 /////////////////////////////////////////////////////////////////////////////////////////////
 #endregion

 #region Button Enabling Visualization Bug
 /////////////////////////////////////////////////////////////////////////////////////////////

 private void OnCounterClicked(object sender, EventArgs e) { ClickCount++; }

 public Command IncrementClickCount { get; }

 private void OnIncrementClickCount() { ClickCount++; }

 private bool ValidateIncrementClickCount() { return CanClickCount; }

 public int ClickCount { get => clickCount; set => SetProperty(ref clickCount, value); }
 private int clickCount;

 public bool CanClickCount { get => canClickCount; set => SetProperty(ref canClickCount, value, null, () => IncrementClickCount.ChangeCanExecute()); }
 private bool canClickCount;

 public Command EnableIncrementClickCount { get; }

 private void OnEnableIncrementClickCount() { CanClickCount = true; }

 public Command DisableIncrementClickCount { get; }

 private void OnDisableIncrementClickCount() { CanClickCount = false; }

 /////////////////////////////////////////////////////////////////////////////////////////////
 #endregion

 #region Display Missing Buttons Bug
 /////////////////////////////////////////////////////////////////////////////////////////////

 public Command Download { get; }

 private void OnDownload()
 {
  string serverPath = FtpPathCombine(FtpHost, FileName);
  string message = DownloadFile(serverPath, TempApplicationDatabasePath, false, FtpCredentials);

  if (string.IsNullOrEmpty(message))
   DisplayAlert("Database Restore Successful", "The database has been successfully restored. The application will restart.", "OK");
  else
   DisplayAlert("Database Restore Failed", $"The database restore failed. Message:\n\n{message}", "OK");
 }

 private static string TempApplicationDatabasePath
 { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), FileName);

 public FtpsCredentials FtpCredentials
 { get; } = new FtpsCredentials($"{FtpAccount}@murraycontrolpro.com", FtpPassword);

 private const string FileName = "registers.db3";

 private const string FtpAccount = "MauiTesting";

 private const string FtpPassword = "2[x1iAQC-dRo";

 private const string FtpHost = @"ftp://murraycontrolpro.com";

 /////////////////////////////////////////////////////////////////////////////////////////////
 #endregion

 #region INotifyPropertyChanged
 /////////////////////////////////////////////////////////////////////////////////////////////

 protected bool SetProperty<T>(
  ref T backingStore,
  T value,
  string[] otherPropertyNames = null,
  Action onChanged = null,
  [CallerMemberName] string propertyName = "")
 {
  if (EqualityComparer<T>.Default.Equals(backingStore, value))
   return false;

  backingStore = value;
  onChanged?.Invoke();
  OnPropertyChanged(propertyName);
  if (otherPropertyNames != null)
   foreach (string otherPropertyName in otherPropertyNames)
    OnPropertyChanged(otherPropertyName);
  return true;
 }

 /////////////////////////////////////////////////////////////////////////////////////////////
 #endregion

 #region From FTP Utilities
 /////////////////////////////////////////////////////////////////////////////////////////////

 /// <summary>
 /// Downloads a single file.
 /// </summary>
 /// <returns>Successful if returned message is null</returns>
 public static string DownloadFile(
  string serverPath,
  string localPath,
  bool deleteAfterDownload,
  FtpsCredentials credentials)
 {
  try
  {
   long fileSize = GetFileSize(serverPath, credentials);
   long bytesRead = 0;
   FtpWebRequest request = CreateRequest(serverPath, WebRequestMethods.Ftp.DownloadFile, credentials);
   using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
   using (Stream responseStream = response.GetResponseStream())
   using (FileStream fileStream = File.Create(localPath))
   {
    byte[] buffer = new byte[32768]; // 128 will work most of the time.
    while (true)
    {
     int read = responseStream.Read(buffer, 0, buffer.Length);
     if (read <= 0)
      break;
     bytesRead += read;
     fileStream.Write(buffer, 0, read);
    }
   }

   FileInfo fileInfo = new FileInfo(localPath);
   if (fileSize != bytesRead)
    return $"File size mismatch. FTP: {fileSize:n0}; local: {bytesRead:n0}";

   //if (deleteAfterDownload)
   // DeleteFile(serverPath, credentials);
  }
  catch (Exception e)
  {
   string message = e.Message;// GetInnerMessage();
   return message;
  }
  return null;
 }

 /// <summary>
 /// Combines the server path and file name.
 /// </summary>
 public static string FtpPathCombine(string serverPath, string file) => $"{serverPath}/{file}";

 /// <summary>
 /// Gets the size of a file on the server.
 /// </summary>
 /// <returns>Bytes</returns>
 private static long GetFileSize(string serverPath, FtpsCredentials credentials)
 {
  long fileSize = -1;
  FtpWebRequest request = CreateRequest(serverPath, WebRequestMethods.Ftp.GetFileSize, credentials);
  using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
  {
   fileSize = response.ContentLength;
   response.Close();
  }
  return fileSize;
 }

 /// <summary>
 /// Timeout in milliseconds.
 /// </summary>
 public static int RequestTimeOut = 5000;

 /// <summary>
 /// Creates an FTP request.
 /// </summary>
 private static FtpWebRequest CreateRequest(string serverPath, string method, FtpsCredentials credentials)
 {
  FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(serverPath));
  request.Credentials = new NetworkCredential(credentials.UserName, credentials.Password);
  request.UseBinary = true;
  request.UsePassive = true;
  request.Proxy = null;
  request.KeepAlive = false;
  request.Method = method;
  request.Timeout = RequestTimeOut;
  return request;
 }

 /// <summary>
 /// Credentials for logging into an SFTP server.
 /// </summary>
 public class FtpsCredentials
 {
  public FtpsCredentials(string userName, string password)
  {
   UserName = userName;
   Password = password;
  }

  public string UserName;
  public string Password;
 }

 /////////////////////////////////////////////////////////////////////////////////////////////
 #endregion
}

