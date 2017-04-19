using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace EdytorXML_New
{
	class Plik
	{
		private string zawartosc; // zawartość TextBoxa
		// obiekty przechowujące strukturę pliku xml
		XDocument xDokument;
		XmlDocument xmlDokument;
		StorageFile plik;
		// Zmienna pomocnicza sygnalizująca wystąpienie wyjątku
		bool isException;

		// Konstruktor inicjalizuje pola klasy
		public Plik()
		{
			zawartosc = null;
			xDokument = null;
			xmlDokument = null;
			plik = null;
			isException = false;
		}
		
		// Zapis pliku
		public async void zapisz(TextBox textBox)
		{
			// Użycie FileSavePickera w celu zapisania pliku w wybranej przez użytkownika lokalizacji
			FileSavePicker savePicker = new FileSavePicker();
			// Domyślna lokalizacja - DocumentsLibrary
			savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
			// Typ pliku - dozwolony tylko format XML
			savePicker.FileTypeChoices.Add("XML File", new List<string>() { ".xml" });
			// Sugerowana domyślna nazwa pliku
			savePicker.SuggestedFileName = "Nowy";

			plik = await savePicker.PickSaveFileAsync();

			// Sprawdzenie czy użytkownik wskazał plik do zapisu
			if (plik != null)
			{
				try
				{
					// Zapisanie zawartości textboxa do zmiennej oraz parsowanie do obiektu XDocument
					zawartosc = textBox.Text;
					xDokument = XDocument.Parse(zawartosc);

					// Zapis pliku przy użyciu strumienia
					using (IRandomAccessStream fileStream = await plik.OpenAsync(FileAccessMode.ReadWrite))
					{
						using (IOutputStream outputStream = fileStream.GetOutputStreamAt(0))
						{
							using (DataWriter dataWriter = new DataWriter(outputStream))
							{
								dataWriter.WriteString(xDokument.ToString());
								await dataWriter.StoreAsync();
								dataWriter.DetachStream();
							}
							await outputStream.FlushAsync();
						}
					}
				}
				catch (Exception e)
				{
					// Sygnalizacja wystąpienia wyjątku
					isException = true;
				}
				// Obsługa wyjątku
				if (isException)
				{
					// Wyświetlenie wiadomości w oknie dialogowym
					var messageDialog = new MessageDialog("Nieprawidłowa struktura pliku XML. Plik nie został zapisany.");
					await messageDialog.ShowAsync();
				}
			}
		}

		// Odczyt z pliku
		public async void wczytaj(TextBox textBox)
		{
			FileOpenPicker openPicker = new FileOpenPicker();
			openPicker.ViewMode = PickerViewMode.List;
			openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
			openPicker.FileTypeFilter.Add(".xml");
			plik = await openPicker.PickSingleFileAsync();

			// Sprawdzenie czy użytkownik wybrał plik do wczytania
			if (plik != null)
			{
				// Próba załadowania textboxa wczytanym plikiem
				try
				{
					xmlDokument = await XmlDocument.LoadFromFileAsync(plik);
					zawartosc = xmlDokument.GetXml();
					xDokument = XDocument.Parse(zawartosc);
					textBox.Text = xDokument.ToString();
				}
				catch (Exception e)
				{
					isException = true;
				}

				if (isException)
				{
					var messageDialog = new MessageDialog("Nieprawidłowa struktura pliku XML. Wybierz inny plik");
					await messageDialog.ShowAsync();
					isException = false;
				}
			}
		}
	}
}
