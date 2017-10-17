using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;

namespace WebMotors.Components.Replicator.Implementations.Replicators
{
	public class ElasticSearch : IReplicator
	{
		#region [ IReplicator ]
		public void insert<T>(T pk, string table, string json, Constants constants)
		{
			Send(string.Format("{0}/{1}/{2}", constants.elasticSearchUrl, table, pk.ToString()), json);
		}

		public void update<T>(T pk, string table, string json, Constants constants)
		{
			Send(string.Format("{0}/{1}/{2}", constants.elasticSearchUrl, table, pk.ToString()), json);
		}

		public void delete<T>(T pk, string table, Constants constants)
		{
			Remove(string.Format("{0}/{1}/{2}", constants.elasticSearchUrl, table, pk.ToString()));
		}
		#endregion

		#region [ private ]
		private void Send(string url, string json)
		{
			string retorno = "";
			if (!string.IsNullOrEmpty(json))
			{
				var http = HttpConfiguration(url, HttpMethod.Put);
				Byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

				bool erro = RequestStream(http, bytes);
				if (!erro)
					retorno = GetResponse(http);
			}
			CoreModel.Log(retorno);
		}

		private void Remove(string url)
		{
			CoreModel.Log(GetResponse(HttpConfiguration(url, HttpMethod.Delete)));
		}
		#endregion

		#region[Helpers]
		private static HttpWebRequest HttpConfiguration(string url, HttpMethod method)
		{
			var http = (HttpWebRequest)WebRequest.Create(new Uri(url));
			http.ContentType = Constants.contentType;
			http.Method = method.ToString();
			return http;
		}
		private static string GetResponse(HttpWebRequest http)
		{
			string retorno;
			try
			{
				using (var response = http.GetResponse())
				{
					try
					{
						var stream = response.GetResponseStream();
						var sr = new StreamReader(stream);
						retorno = sr.ReadToEnd();
					}
					catch (Exception ex)
					{
						retorno = "Erro";
						CoreModel.LogException(ex);
					}
				}
			}
			catch (Exception ex)
			{
				retorno = "Erro";
				CoreModel.LogException(ex);
			}
			return retorno;
		}
		private static bool RequestStream(HttpWebRequest http, Byte[] bytes)
		{
			bool erro = false;
			using (var stream = http.GetRequestStream())
			{
				try
				{
					stream.Write(bytes, 0, bytes.Length);
					stream.Close();
				}
				catch (Exception ex)
				{
					erro = true;
					CoreModel.LogException(ex);
				}
			}
			return erro;
		}
		#endregion
	}

	internal class ResponseModel
	{
		public string _index { get; set; }
		public string _type { get; set; }
		public string _id { get; set; }
		public string _version { get; set; }
		public _shards _shards { get; set; }
		public bool created { get; set; }
	}
	internal class _shards
	{
		public int total { get; set; }
		public int successsful { get; set; }
		public int failed { get; set; }
	}
}
