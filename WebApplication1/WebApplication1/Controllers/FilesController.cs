using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class FilesController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env;
        private string _saveFileFolder;

        public FilesController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
            _saveFileFolder = _env.WebRootPath + "\\uploads\\file";
        }
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// FILE IS IMAGE OR PDF
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UploadFile()
        {
            List<Files> filelist = new List<Files>();
            string CS = _configuration["connectionString"].ToString();
            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Files", con);
                //cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Files f = new Files();
                    f.ID = Convert.ToInt32(rdr["ID"]);
                    f.Name = rdr["Name"].ToString();
                    f.FileSize = Convert.ToInt32(rdr["FileSize"]);
                    f.FilePath = rdr["FilePath"].ToString();

                    filelist.Add(f);
                }
            }
            return View(filelist);
        }
        [HttpPost]
        public ActionResult UploadFile(IFormFile fileupload)
        {
            if (fileupload != null)
            {
                string fileName = Path.GetFileName(fileupload.FileName);
                int fileSize = (int)fileupload.Length;
                int Size = (int)fileSize / 1000;
                // if validate type video
                List<string> ImageTypeList = new List<string> { ".jpg", ".png", ".jpeg", ".svg", ".gif" };
                List<string> AllowTypeVideo = new List<string> { ".mp4" };
                string filename = "";
                // create file path
                string extension = Path.GetExtension(fileupload.FileName);
                string filePath = Path.Combine(_saveFileFolder);
                //check exists
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                //
                filePath = Path.Combine(filePath, fileName);
                //upload file
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        fileupload.CopyToAsync(stream).Wait();
                        //filename = Path.Combine(subFolder, fileName);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                string CS = _configuration["connectionString"].ToString();
                using (SqlConnection con = new SqlConnection(CS))
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO Files VALUES (@Name, @FileSize, @FilePath)", con);
                    //cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    cmd.Parameters.AddWithValue("@Name", fileName);
                    cmd.Parameters.AddWithValue("@FileSize", Size);
                    cmd.Parameters.AddWithValue("@FilePath",  fileName);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("UploadFile");
        }
    }
}
