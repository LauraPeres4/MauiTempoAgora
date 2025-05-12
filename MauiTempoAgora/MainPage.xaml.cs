using MauiTempoAgora.Model;
using MauiTempoAgora.Services;
using System.Diagnostics;

namespace MauiTempoAgora
{
    public partial class MainPage : ContentPage // Classe principal da tela, herda de ContentPage
    {
        public MainPage()
        {
            InitializeComponent(); // Inicializa os componentes da interface
        }

        // Evento disparado ao clicar no botão de previsão
        private async void Button_Clicked_Previsao(object sender, EventArgs e)
        {
            try
            {
                // Verifica se o campo de cidade foi preenchido
                if (!string.IsNullOrEmpty(txt_cidade.Text))
                {
                    // Chama o serviço para buscar os dados do tempo com base na cidade
                    Tempo? t = await DataService.GetPrevisao(txt_cidade.Text);

                    if (t != null)
                    {
                        // Monta uma string com os dados da previsão
                        string dados_previsao = "";

                        dados_previsao = $"Latitude: {t.lat} \n" +
                                         $"Longitude: {t.lon} \n" +
                                         $"Nascer do sol: {t.sunrise} \n" +
                                         $"Por do sol: {t.sunset} \n" +
                                         $"Temp max: {t.temp_max} \n" +
                                         $"Temp min: {t.temp_min} \n";

                        // Exibe os dados no rótulo da interface
                        lbl_res.Text = dados_previsao;

                        // Monta a URL do mapa Windy usando as coordenadas
                        string mapa = $"https://embed.windy.com/embed.html?" +
                                      $"type=map&location=coordinates&metricRain=mm&metricTemp=°C" +
                                      $"&metricWind=km/h&zoom=5&overlay=wind&product=ecmwf&level=surface" +
                                      $"&lat={t.lat.ToString().Replace(",", ".")}&lon=" +
                                      $"{t.lon.ToString().Replace(",", ".")}";

                        // Define a fonte do WebView para exibir o mapa
                        wv_mapa.Source = mapa;

                        // Escreve a URL no console para depuração
                        Debug.WriteLine(mapa);
                    }
                    else
                    {
                        // Caso não retorne dados, informa o usuário
                        lbl_res.Text = "Sem dados de previsão";
                    }
                }
            }
            catch (Exception ex)
            {
                // Captura qualquer erro e exibe uma mensagem de alerta
                await DisplayAlert("Ops", ex.Message, "OK");
            }
        }

        // Evento disparado ao clicar no botão de localização
        private async void Button_Clicked_Localizacao(object sender, EventArgs e)
        {
            try
            {
                // Cria uma requisição para obter a localização com precisão média
                GeolocationRequest request = new GeolocationRequest(
                    GeolocationAccuracy.Medium,
                    TimeSpan.FromSeconds(10)
                );

                // Tenta obter a localização atual
                Location? local = await Geolocation.Default.GetLocationAsync(request);

                if (local != null)
                {
                    // Mostra a latitude e longitude na interface
                    string local_disp = $"Latitude: {local.Latitude}\n" +
                                        $"Longitude: {local.Longitude}";

                    lbl_coords.Text = local_disp;

                    // Busca o nome da cidade baseado nas coordenadas
                    GetCidade(local.Latitude, local.Longitude);
                }
                else
                {
                    // Caso a localização seja nula
                    lbl_coords.Text = "Nenhuma localização";
                }
            }
            // Trata exceção caso o dispositivo não suporte geolocalização
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("Erro: Dispositivo não Suporta", fnsEx.Message, "OK");
            }
            // Trata exceção caso a localização esteja desativada
            catch (FeatureNotEnabledException fneEx)
            {
                await DisplayAlert("Erro: Localização Desabilitada", fneEx.Message, "OK");
            }
            // Trata exceção caso não tenha permissão de localização
            catch (PermissionException pEx)
            {
                await DisplayAlert("Erro: Permissão da Localização", pEx.Message, "OK");
            }
            // Trata outras exceções genéricas
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        // Método que obtém o nome da cidade com base na latitude e longitude
        private async void GetCidade(double lat, double lon)
        {
            try
            {
                // Chama o serviço de geocodificação reversa
                IEnumerable<Placemark> places = await Geocoding.Default.GetPlacemarksAsync(lat, lon);

                Placemark? place = places.FirstOrDefault();

                if (place != null)
                {
                    // Se encontrar um local válido, preenche o campo de cidade
                    txt_cidade.Text = place.Locality;
                }
            }
            catch (Exception ex)
            {
                // Trata erro caso não consiga obter a cidade
                await DisplayAlert("Erro: obtenção do nome da cidade", ex.Message, "OK");
            }
        }
    }
}
