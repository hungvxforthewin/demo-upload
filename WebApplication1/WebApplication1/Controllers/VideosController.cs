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
    public class VideosController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env;
        private string _saveFileFolder;

        public VideosController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
            _saveFileFolder = _env.WebRootPath + "\\uploads\\video";
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult UploadVideo()
        {
            List<VideoFiles> videolist = new List<VideoFiles>();
            string CS = _configuration["connectionString"].ToString(); 
            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlCommand cmd = new SqlCommand("spGetAllVideoFile", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    VideoFiles video = new VideoFiles();
                    video.ID = Convert.ToInt32(rdr["ID"]);
                    video.Name = rdr["Name"].ToString();
                    video.FileSize = Convert.ToInt32(rdr["FileSize"]);
                    video.FilePath = rdr["FilePath"].ToString();

                    videolist.Add(video);
                }
            }
            return View(videolist);
        }
        [HttpPost]
        public ActionResult UploadVideo(IFormFile fileupload)
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
                    SqlCommand cmd = new SqlCommand("spAddNewVideoFile", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    cmd.Parameters.AddWithValue("@Name", fileName);
                    cmd.Parameters.AddWithValue("@FileSize", Size);
                    cmd.Parameters.AddWithValue("FilePath", "~/uploads/video/" + fileName);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("UploadVideo");
        }
    }
}
