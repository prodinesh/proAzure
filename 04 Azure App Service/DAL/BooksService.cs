﻿using AzureSQL_ServiceApp.Interface;
using AzureSQL_ServiceApp.Model;
using Microsoft.Data.SqlClient;
using Microsoft.FeatureManagement;
using System.Diagnostics.Eventing.Reader;
using static System.Net.Mime.MediaTypeNames;

namespace AzureSQL_ServiceApp.DAL
{
    public class BooksService : IBooksService
    {
        
        private readonly IConfiguration _configuration;
        private readonly IFeatureManager _featureManager;

        private SqlConnection GetSQLDBConnection()
        {
            //Connection string from Local DB
            string strConnection = "Server=DINESH-O\\SQLEXPRESS;database=Experts;Trusted_Connection=True;TrustServerCertificate=True";

            //Connection string from App Config service
            //SqlConnection conn = new SqlConnection(_configuration["SQLConnection"]);

            return new SqlConnection(strConnection);
        }
        
        public BooksService(IConfiguration configuration, IFeatureManager featureManager) 
        { 
            _configuration = configuration;
            _featureManager = featureManager;
        }

        public List<Books> GetBooks()
        {
            List<Books> listBooks = new List<Books>();
            string query = "select * from Books order by Book_id";
            SqlConnection conn = GetSQLDBConnection();
            conn.Open();
            SqlCommand cmd = new SqlCommand(query, conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Books book = new Books();
                    book.BookId = reader.GetInt32(0);
                    book.GuidId = reader.GetGuid(1);
                    book.Title = reader.GetString(2);
                    book.Author = reader.GetString(3);
                    book.Genere = reader.GetString(4);
                    book.IsFiction = reader.GetBoolean(5);
                    book.Price = reader.GetDecimal(6);
                    book.PublishedDate = reader.GetDateTime(7);
                    string encodedCoverBytes = Convert.ToBase64String((byte[])reader[8]);
                    book.ImgUrl = string.Concat("data:image/jpg;base64,", encodedCoverBytes);
                    listBooks.Add(book);
                }
            }
            conn.Close();
            return listBooks;
        }

        public async Task<bool> IsBeta()
        {
            if (await _featureManager.IsEnabledAsync("Beta"))
                return true;
            else
                return false;
        }
    }
}