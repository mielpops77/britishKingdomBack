using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using British_Kingdom_back.Models; // Assurez-vous d'importer correctement votre modèle
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;

namespace British_Kingdom_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannerController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private readonly BlobServiceClient _blobServiceClient;

        public BannerController(IWebHostEnvironment environment, IConfiguration configuration, BlobServiceClient blobServiceClient)
        {
            _configuration = configuration;
            _environment = environment;
            _blobServiceClient = blobServiceClient;
        }


        [Authorize]
        [HttpPost]
        public IActionResult CreateBannerSection(BannerSection bannerSection)
        {


            if (bannerSection == null)
            {
                return BadRequest(new { message = "bannerSection data is missing." });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();


                using (var command = new SqlCommand("INSERT INTO BannerSection (BannerImages, MaleDescription, KittenDescription, FemaleDescription, Title, Subtitle, MaleImg, KittenImg, FemaleImg, TitleFontSize, TitleFontFamily, SubtitleFontSize, SubtitleFontFamily, ColorHeader, TitleColor, SubtitleColor, TitleCard1, TitleCard2, TitleCard3, Menu, ColorMenu, HoverColorMenu, FontStyleMenu, ColorFooter, Favicon, TitlePageMales, TitleFontStylePageMales, TitleColorPageMales, TextPageMales, TextFontStylePageMales, TextColorPageMales, BordureColorPageMales, TitlePageFemelles, TitleFontStylePageFemelles, TitleColorPageFemelles, TextPageFemelles, TextFontStylePageFemelles, TextColorPageFemelles, BordureColorPageFemelles, TitleFontStylePageChatons, TitleColorPageChatons, Info1FontStylePageChatons, Info1ColorPageChatons, Info2FontStylePageChatons, Info2ColorPageChatons, Info3FontStylePageChatons, Info3ColorPageChatons, BordureColorPageChatons, ButtonColorPageChatons, ButtonTextColorPageChatons, ButtonTextFontStylePageChatons, TextPageContact, TextPageCondition, TitlePagelivreDor, TitleFontStylePagelivreDor, TitleColorPagelivreDor, ButtonColorPagelivreDor, ButtonTextColorPagelivreDor, ButtonTextFontStylePagelivreDor, TitleFontStylePagechatProfil, TitleColorPagechatProfil, TextFontStylePagechatProfil, TextColorPagechatProfil, ButtonColorPagechatProfil, ButtonTextColorPagechatProfil, ButtonTextFontStylePagechatProfil, BordureColorPagechatProfil, BagroundColorPagechatProfil, TitleFontStylePagechatonProfil, TitleColorPagechatonProfil, TextFontStylePagechatonProfil, TextColorPagechatonProfil, StatusNameFontStylePagechatonProfil, StatusNameColorPagechatonProfil, BreedFontStylePagechatonProfil, BreedColorPagechatonProfil, BackgroundColorBreedPagechatonProfil, TitleFontStyleCard, TitleColorCard, TextFontStyleCard, TextColorCard, BackgroundColorCard, ProfilId,TextPageAccueil, TextFontStylePageAccueil, TextColorPageAccueil) VALUES " +
                 "(@BannerImages, @MaleDescription, @KittenDescription, @FemaleDescription, @Title, @Subtitle, @MaleImg, @KittenImg, @FemaleImg, @TitleFontSize, @TitleFontFamily, @SubtitleFontSize, @SubtitleFontFamily, @ColorHeader, @TitleColor, @SubtitleColor,@TitleCard1, @TitleCard2, @TitleCard3, @Menu, @ColorMenu, @HoverColorMenu, @FontStyleMenu, @ColorFooter, @Favicon, @TitlePageMales, @TitleFontStylePageMales, @TitleColorPageMales, @TextPageMales, @TextFontStylePageMales, @TextColorPageMales, @BordureColorPageMales, @TitlePageFemelles, @TitleFontStylePageFemelles, @TitleColorPageFemelles, @TextPageFemelles, @TextFontStylePageFemelles, @TextColorPageFemelles, @BordureColorPageFemelles, @TitleFontStylePageChatons, @TitleColorPageChatons, @Info1FontStylePageChatons, @Info1ColorPageChatons, @Info2FontStylePageChatons, @Info2ColorPageChatons, @Info3FontStylePageChatons, @Info3ColorPageChatons, @BordureColorPageChatons, @ButtonColorPageChatons, @ButtonTextColorPageChatons, @ButtonTextFontStylePageChatons, @TextPageContact, @TextPageCondition, @TitlePagelivreDor, @TitleFontStylePagelivreDor, @TitleColorPagelivreDor, @ButtonColorPagelivreDor, @ButtonTextColorPagelivreDor, @ButtonTextFontStylePagelivreDor, @TitleFontStylePagechatProfil, @TitleColorPagechatProfil, @TextFontStylePagechatProfil, @TextColorPagechatProfil, @ButtonColorPagechatProfil, @ButtonTextColorPagechatProfil, @ButtonTextFontStylePagechatProfil, @BordureColorPagechatProfil, @BagroundColorPagechatProfil, @TitleFontStylePagechatonProfil, @TitleColorPagechatonProfil, @TextFontStylePagechatonProfil, @TextColorPagechatonProfil, @StatusNameFontStylePagechatonProfil, @StatusNameColorPagechatonProfil, @BreedFontStylePagechatonProfil, @BreedColorPagechatonProfil, @BackgroundColorBreedPagechatonProfil, @TitleFontStyleCard, @TitleColorCard, @TextFontStyleCard, @TextColorCard, @BackgroundColorCard, @profilId, @TextPageAccueil, @TextFontStylePageAccueil, @TextColorPageAccueil)", connection))
                {


                    var bannerImagesString = string.Join(",", bannerSection.BannerImages!);
                    var menuString = bannerSection.Menu != null ? string.Join(",", bannerSection.Menu) : null;
                    command.Parameters.AddWithValue("@ProfilId", bannerSection.ProfilId);
                    command.Parameters.AddWithValue("@BannerImages", bannerImagesString);
                    command.Parameters.AddWithValue("@MaleDescription", bannerSection.MaleDescription);
                    command.Parameters.AddWithValue("@KittenDescription", bannerSection.KittenDescription);
                    command.Parameters.AddWithValue("@FemaleDescription", bannerSection.FemaleDescription);
                    command.Parameters.AddWithValue("@TitleFontStyleCard", bannerSection.TitleFontStyleCard);
                    command.Parameters.AddWithValue("@TitleColorCard", bannerSection.TitleColorCard);
                    command.Parameters.AddWithValue("@TextFontStyleCard", bannerSection.TextFontStyleCard);
                    command.Parameters.AddWithValue("@TextColorCard", bannerSection.TextColorCard);
                    command.Parameters.AddWithValue("@BackgroundColorCard", bannerSection.BackgroundColorCard);
                    command.Parameters.AddWithValue("@TitleCard1", bannerSection.TitleCard1);
                    command.Parameters.AddWithValue("@TitleCard2", bannerSection.TitleCard2);
                    command.Parameters.AddWithValue("@TitleCard3", bannerSection.TitleCard3);
                    command.Parameters.AddWithValue("@Title", bannerSection.Title);
                    command.Parameters.AddWithValue("@Subtitle", bannerSection.Subtitle);
                    command.Parameters.AddWithValue("@MaleImg", bannerSection.MaleImg);
                    command.Parameters.AddWithValue("@KittenImg", bannerSection.KittenImg);
                    command.Parameters.AddWithValue("@FemaleImg", bannerSection.FemaleImg);
                    command.Parameters.AddWithValue("@TitleFontSize", bannerSection.TitleFontSize);
                    command.Parameters.AddWithValue("@TitleFontFamily", bannerSection.TitleFontFamily);
                    command.Parameters.AddWithValue("@SubtitleFontSize", bannerSection.SubtitleFontSize);
                    command.Parameters.AddWithValue("@SubtitleFontFamily", bannerSection.SubtitleFontFamily);
                    command.Parameters.AddWithValue("@ColorHeader", bannerSection.ColorHeader);
                    command.Parameters.AddWithValue("@TitleColor", bannerSection.TitleColor);
                    command.Parameters.AddWithValue("@SubtitleColor", bannerSection.SubtitleColor);
                    command.Parameters.AddWithValue("@ColorMenu", bannerSection.ColorMenu);
                    command.Parameters.AddWithValue("@HoverColorMenu", bannerSection.HoverColorMenu);
                    command.Parameters.AddWithValue("@FontStyleMenu", bannerSection.FontStyleMenu);
                    command.Parameters.AddWithValue("@Menu", menuString);
                    command.Parameters.AddWithValue("@ColorFooter", bannerSection.ColorFooter);
                    command.Parameters.AddWithValue("@Favicon", bannerSection.Favicon);

                    command.Parameters.AddWithValue("@TitlePageMales", bannerSection.TitlePageMales);
                    command.Parameters.AddWithValue("@TitleFontStylePageMales", bannerSection.TitleFontStylePageMales);
                    command.Parameters.AddWithValue("@TitleColorPageMales", bannerSection.TitleColorPageMales);

                    command.Parameters.AddWithValue("@TextPageMales", bannerSection.TextPageMales);
                    command.Parameters.AddWithValue("@TextFontStylePageMales", bannerSection.TextFontStylePageMales);
                    command.Parameters.AddWithValue("@TextColorPageMales", bannerSection.TextColorPageMales);

                    command.Parameters.AddWithValue("@BordureColorPageMales", bannerSection.BordureColorPageMales);

                    command.Parameters.AddWithValue("@TitlePageFemelles", bannerSection.TitlePageFemelles);
                    command.Parameters.AddWithValue("@TitleFontStylePageFemelles", bannerSection.TitleFontStylePageFemelles);
                    command.Parameters.AddWithValue("@TitleColorPageFemelles", bannerSection.TitleColorPageFemelles);

                    command.Parameters.AddWithValue("@TextPageFemelles", bannerSection.TextPageFemelles);
                    command.Parameters.AddWithValue("@TextFontStylePageFemelles", bannerSection.TextFontStylePageFemelles);
                    command.Parameters.AddWithValue("@TextColorPageFemelles", bannerSection.TextColorPageFemelles);

                    command.Parameters.AddWithValue("@BordureColorPageFemelles", bannerSection.BordureColorPageFemelles);



                    command.Parameters.AddWithValue("@TitleFontStylePageChatons", bannerSection.TitleFontStylePageChatons);
                    command.Parameters.AddWithValue("@TitleColorPageChatons", bannerSection.TitleColorPageChatons);

                    command.Parameters.AddWithValue("@Info1FontStylePageChatons", bannerSection.Info1FontStylePageChatons);
                    command.Parameters.AddWithValue("@Info1ColorPageChatons", bannerSection.Info1ColorPageChatons);

                    command.Parameters.AddWithValue("@Info2FontStylePageChatons", bannerSection.Info2FontStylePageChatons);
                    command.Parameters.AddWithValue("@Info2ColorPageChatons", bannerSection.Info2ColorPageChatons);

                    command.Parameters.AddWithValue("@Info3FontStylePageChatons", bannerSection.Info3FontStylePageChatons);
                    command.Parameters.AddWithValue("@Info3ColorPageChatons", bannerSection.Info3ColorPageChatons);

                    command.Parameters.AddWithValue("@BordureColorPageChatons", bannerSection.BordureColorPageChatons);
                    command.Parameters.AddWithValue("@ButtonColorPageChatons", bannerSection.ButtonColorPageChatons);
                    command.Parameters.AddWithValue("@ButtonTextColorPageChatons", bannerSection.ButtonTextColorPageChatons);
                    command.Parameters.AddWithValue("@ButtonTextFontStylePageChatons", bannerSection.ButtonTextFontStylePageChatons);
                    command.Parameters.AddWithValue("@TextPageContact", bannerSection.TextPageContact);
                    command.Parameters.AddWithValue("@TextPageCondition", bannerSection.TextPageCondition);

                    command.Parameters.AddWithValue("@TitlePagelivreDor", bannerSection.TitlePagelivreDor);
                    command.Parameters.AddWithValue("@TitleFontStylePagelivreDor", bannerSection.TitleFontStylePagelivreDor);
                    command.Parameters.AddWithValue("@TitleColorPagelivreDor", bannerSection.TitleColorPagelivreDor);
                    command.Parameters.AddWithValue("@ButtonColorPagelivreDor", bannerSection.ButtonColorPagelivreDor);
                    command.Parameters.AddWithValue("@ButtonTextColorPagelivreDor", bannerSection.ButtonTextColorPagelivreDor);
                    command.Parameters.AddWithValue("@ButtonTextFontStylePagelivreDor", bannerSection.ButtonTextFontStylePagelivreDor);

                    command.Parameters.AddWithValue("@TitleFontStylePagechatProfil", bannerSection.TitleFontStylePagechatProfil);
                    command.Parameters.AddWithValue("@TitleColorPagechatProfil", bannerSection.TitleColorPagechatProfil);
                    command.Parameters.AddWithValue("@TextFontStylePagechatProfil", bannerSection.TextFontStylePagechatProfil);
                    command.Parameters.AddWithValue("@TextColorPagechatProfil", bannerSection.TextColorPagechatProfil);
                    command.Parameters.AddWithValue("@ButtonColorPagechatProfil", bannerSection.ButtonColorPagechatProfil);
                    command.Parameters.AddWithValue("@ButtonTextColorPagechatProfil", bannerSection.ButtonTextColorPagechatProfil);
                    command.Parameters.AddWithValue("@ButtonTextFontStylePagechatProfil", bannerSection.ButtonTextFontStylePagechatProfil);
                    command.Parameters.AddWithValue("@BordureColorPagechatProfil", bannerSection.BordureColorPagechatProfil);
                    command.Parameters.AddWithValue("@BagroundColorPagechatProfil", bannerSection.BagroundColorPagechatProfil);

                    command.Parameters.AddWithValue("@TitleFontStylePagechatonProfil", bannerSection.TitleFontStylePagechatonProfil);
                    command.Parameters.AddWithValue("@TitleColorPagechatonProfil", bannerSection.TitleColorPagechatonProfil);
                    command.Parameters.AddWithValue("@TextFontStylePagechatonProfil", bannerSection.TextFontStylePagechatonProfil);
                    command.Parameters.AddWithValue("@TextColorPagechatonProfil", bannerSection.TextColorPagechatonProfil);
                    command.Parameters.AddWithValue("@StatusNameFontStylePagechatonProfil", bannerSection.StatusNameFontStylePagechatonProfil);
                    command.Parameters.AddWithValue("@StatusNameColorPagechatonProfil", bannerSection.StatusNameColorPagechatonProfil);
                    command.Parameters.AddWithValue("@BreedFontStylePagechatonProfil", bannerSection.BreedFontStylePagechatonProfil);
                    command.Parameters.AddWithValue("@BreedColorPagechatonProfil", bannerSection.BreedColorPagechatonProfil);
                    command.Parameters.AddWithValue("@BackgroundColorBreedPagechatonProfil", bannerSection.BackgroundColorBreedPagechatonProfil);
                    command.Parameters.AddWithValue("@TextPageAccueil", bannerSection.TextPageAccueil);
                    command.Parameters.AddWithValue("@TextFontStylePageAccueil", bannerSection.TextFontStylePageAccueil);
                    command.Parameters.AddWithValue("@TextColorPageAccueil", bannerSection.TextColorPageAccueil);



                    command.ExecuteNonQuery();
                }
                return CreatedAtAction(nameof(GetAllBannerSection), bannerSection);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBannerSection()
        {

            var profilId = "";

            if (HttpContext.Request.Query.ContainsKey("profilId"))
            {
                profilId = HttpContext.Request.Query["profilId"].ToString();
            }
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var bannerSections = new List<BannerSection>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT * FROM BannerSection WHERE profilId = @profilId", connection))
                {
                    command.Parameters.AddWithValue("@profilId", profilId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var bannerImagesString = reader.GetString(reader.GetOrdinal("BannerImages"));

                            var bannerImagesArray = !string.IsNullOrEmpty(bannerImagesString)
                                ? bannerImagesString.Split(',')
                                : new string[0];

                            var bannerSection = new BannerSection
                            {
                                ProfilId = reader.IsDBNull(reader.GetOrdinal("ProfilId")) ? 0 : reader.GetInt32(reader.GetOrdinal("ProfilId")),
                                BannerImages = bannerImagesArray,
                                MaleDescription = GetStringOrNull(reader, "MaleDescription"),
                                KittenDescription = GetStringOrNull(reader, "KittenDescription"),
                                FemaleDescription = GetStringOrNull(reader, "FemaleDescription"),
                                Title = GetStringOrNull(reader, "Title"),
                                Subtitle = GetStringOrNull(reader, "Subtitle"),
                                MaleImg = GetStringOrNull(reader, "MaleImg"),
                                KittenImg = GetStringOrNull(reader, "KittenImg"),
                                FemaleImg = GetStringOrNull(reader, "FemaleImg"),
                                TitleFontStyleCard = GetStringOrNull(reader, "TitleFontStyleCard"),
                                TitleColorCard = GetStringOrNull(reader, "TitleColorCard"),
                                TextFontStyleCard = GetStringOrNull(reader, "TextFontStyleCard"),
                                TextColorCard = GetStringOrNull(reader, "TextColorCard"),
                                BackgroundColorCard = GetStringOrNull(reader, "BackgroundColorCard"),
                                TitleFontSize = reader.IsDBNull(reader.GetOrdinal("TitleFontSize")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("TitleFontSize")),
                                TitleFontFamily = GetStringOrNull(reader, "TitleFontFamily"),
                                SubtitleFontSize = reader.IsDBNull(reader.GetOrdinal("SubtitleFontSize")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("SubtitleFontSize")),
                                SubtitleFontFamily = GetStringOrNull(reader, "SubtitleFontFamily"),
                                ColorHeader = GetStringOrNull(reader, "ColorHeader"),
                                TitleColor = GetStringOrNull(reader, "TitleColor"),
                                SubtitleColor = GetStringOrNull(reader, "SubtitleColor"),
                                TitleCard1 = GetStringOrNull(reader, "TitleCard1"),
                                TitleCard2 = GetStringOrNull(reader, "TitleCard2"),
                                TitleCard3 = GetStringOrNull(reader, "TitleCard3"),
                                Menu = GetStringArrayOrNull(reader, "Menu"),
                                ColorMenu = GetStringOrNull(reader, "ColorMenu"),
                                HoverColorMenu = GetStringOrNull(reader, "HoverColorMenu"),
                                FontStyleMenu = GetStringOrNull(reader, "FontStyleMenu"),
                                ColorFooter = GetStringOrNull(reader, "ColorFooter"),
                                Favicon = GetStringOrNull(reader, "Favicon"),
                                TitlePageMales = GetStringOrNull(reader, "TitlePageMales"),
                                TitleFontStylePageMales = GetStringOrNull(reader, "TitleFontStylePageMales"),
                                TitleColorPageMales = GetStringOrNull(reader, "TitleColorPageMales"),
                                TextPageMales = GetStringOrNull(reader, "TextPageMales"),
                                TextFontStylePageMales = GetStringOrNull(reader, "TextFontStylePageMales"),
                                TextColorPageMales = GetStringOrNull(reader, "TextColorPageMales"),
                                BordureColorPageMales = GetStringOrNull(reader, "BordureColorPageMales"),
                                TitlePageFemelles = GetStringOrNull(reader, "TitlePageFemelles"),
                                TitleFontStylePageFemelles = GetStringOrNull(reader, "TitleFontStylePageFemelles"),
                                TitleColorPageFemelles = GetStringOrNull(reader, "TitleColorPageFemelles"),
                                TextPageFemelles = GetStringOrNull(reader, "TextPageFemelles"),
                                TextFontStylePageFemelles = GetStringOrNull(reader, "TextFontStylePageFemelles"),
                                TextColorPageFemelles = GetStringOrNull(reader, "TextColorPageFemelles"),
                                BordureColorPageFemelles = GetStringOrNull(reader, "BordureColorPageFemelles"),

                                TitleFontStylePageChatons = GetStringOrNull(reader, "TitleFontStylePageChatons"),
                                TitleColorPageChatons = GetStringOrNull(reader, "TitleColorPageChatons"),

                                Info1FontStylePageChatons = GetStringOrNull(reader, "Info1FontStylePageChatons"),
                                Info1ColorPageChatons = GetStringOrNull(reader, "Info1ColorPageChatons"),

                                Info2FontStylePageChatons = GetStringOrNull(reader, "Info2FontStylePageChatons"),
                                Info2ColorPageChatons = GetStringOrNull(reader, "Info2ColorPageChatons"),

                                Info3FontStylePageChatons = GetStringOrNull(reader, "Info3FontStylePageChatons"),
                                Info3ColorPageChatons = GetStringOrNull(reader, "Info3ColorPageChatons"),

                                BordureColorPageChatons = GetStringOrNull(reader, "BordureColorPageChatons"),
                                ButtonColorPageChatons = GetStringOrNull(reader, "ButtonColorPageChatons"),
                                ButtonTextColorPageChatons = GetStringOrNull(reader, "ButtonTextColorPageChatons"),
                                ButtonTextFontStylePageChatons = GetStringOrNull(reader, "ButtonTextFontStylePageChatons"),
                                TextPageContact = GetStringOrNull(reader, "TextPageContact"),
                                TextPageCondition = GetStringOrNull(reader, "TextPageCondition"),
                                TitlePagelivreDor = GetStringOrNull(reader, "TitlePagelivreDor"),
                                TitleFontStylePagelivreDor = GetStringOrNull(reader, "TitleFontStylePagelivreDor"),
                                TitleColorPagelivreDor = GetStringOrNull(reader, "TitleColorPagelivreDor"),
                                ButtonColorPagelivreDor = GetStringOrNull(reader, "ButtonColorPagelivreDor"),
                                ButtonTextColorPagelivreDor = GetStringOrNull(reader, "ButtonTextColorPagelivreDor"),
                                ButtonTextFontStylePagelivreDor = GetStringOrNull(reader, "ButtonTextFontStylePagelivreDor"),
                                TitleFontStylePagechatProfil = GetStringOrNull(reader, "TitleFontStylePagechatProfil"),
                                TitleColorPagechatProfil = GetStringOrNull(reader, "TitleColorPagechatProfil"),
                                TextFontStylePagechatProfil = GetStringOrNull(reader, "TextFontStylePagechatProfil"),
                                TextColorPagechatProfil = GetStringOrNull(reader, "TextColorPagechatProfil"),
                                ButtonColorPagechatProfil = GetStringOrNull(reader, "ButtonColorPagechatProfil"),
                                ButtonTextColorPagechatProfil = GetStringOrNull(reader, "ButtonTextColorPagechatProfil"),
                                ButtonTextFontStylePagechatProfil = GetStringOrNull(reader, "ButtonTextFontStylePagechatProfil"),
                                BordureColorPagechatProfil = GetStringOrNull(reader, "BordureColorPagechatProfil"),
                                BagroundColorPagechatProfil = GetStringOrNull(reader, "BagroundColorPagechatProfil"),
                                TitleFontStylePagechatonProfil = GetStringOrNull(reader, "TitleFontStylePagechatonProfil"),
                                TitleColorPagechatonProfil = GetStringOrNull(reader, "TitleColorPagechatonProfil"),
                                TextFontStylePagechatonProfil = GetStringOrNull(reader, "TextFontStylePagechatonProfil"),
                                TextColorPagechatonProfil = GetStringOrNull(reader, "TextColorPagechatonProfil"),
                                StatusNameFontStylePagechatonProfil = GetStringOrNull(reader, "StatusNameFontStylePagechatonProfil"),
                                StatusNameColorPagechatonProfil = GetStringOrNull(reader, "StatusNameColorPagechatonProfil"),
                                BreedFontStylePagechatonProfil = GetStringOrNull(reader, "BreedFontStylePagechatonProfil"),
                                BreedColorPagechatonProfil = GetStringOrNull(reader, "BreedColorPagechatonProfil"),
                                BackgroundColorBreedPagechatonProfil = GetStringOrNull(reader, "BackgroundColorBreedPagechatonProfil"),
                                TextPageAccueil = GetStringOrNull(reader, "TextPageAccueil"),
                                TextFontStylePageAccueil = GetStringOrNull(reader, "TextFontStylePageAccueil"),
                                TextColorPageAccueil = GetStringOrNull(reader, "TextColorPageAccueil"),
                            };

                            bannerSections.Add(bannerSection);
                        }
                    }
                }
            }

            return Ok(bannerSections);
        }
        private string[] GetStringArrayOrNull(SqlDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader.GetString(reader.GetOrdinal(columnName)).Split(',');
        }
        private string GetStringOrNull(SqlDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader.GetString(reader.GetOrdinal(columnName));
        }


        [Authorize]
        [HttpPut]
        public IActionResult UpdateBannerSection([FromQuery] int profilId, [FromBody] BannerSection updatedBannerSection)
        {

            if (updatedBannerSection == null)
            {
                return BadRequest("Updated banner section data is missing.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("UPDATE BannerSection SET BannerImages = @BannerImages, ProfilId = @ProfilId, MaleDescription = @MaleDescription, KittenDescription = @KittenDescription, FemaleDescription = @FemaleDescription, TitleCard1 = @TitleCard1, TitleCard2 = @TitleCard2, TitleCard3 = @TitleCard3, TitleFontStyleCard = @TitleFontStyleCard, TitleColorCard = @TitleColorCard, TextFontStyleCard = @TextFontStyleCard, TextColorCard = @TextColorCard, BackgroundColorCard = @BackgroundColorCard, Title = @Title, Subtitle = @Subtitle, MaleImg = @MaleImg, KittenImg = @KittenImg, FemaleImg = @FemaleImg, TitleFontSize = @TitleFontSize, TitleFontFamily = @TitleFontFamily, SubtitleFontSize = @SubtitleFontSize, SubtitleFontFamily = @SubtitleFontFamily, ColorHeader=@ColorHeader, TitleColor = @TitleColor, SubtitleColor=@SubtitleColor, Menu = @Menu, ColorMenu = @ColorMenu, HoverColorMenu = @HoverColorMenu, FontStyleMenu = @FontStyleMenu, ColorFooter = @ColorFooter, Favicon = @Favicon, TitlePageMales = @TitlePageMales, TitleFontStylePageMales = @TitleFontStylePageMales, TitleColorPageMales = @TitleColorPageMales, TextPageMales = @TextPageMales, TextFontStylePageMales = @TextFontStylePageMales, TextColorPageMales = @TextColorPageMales, BordureColorPageMales = @BordureColorPageMales, TitlePageFemelles = @TitlePageFemelles, TitleFontStylePageFemelles = @TitleFontStylePageFemelles, TitleColorPageFemelles = @TitleColorPageFemelles, TextPageFemelles = @TextPageFemelles, TextFontStylePageFemelles = @TextFontStylePageFemelles, TextColorPageFemelles = @TextColorPageFemelles, BordureColorPageFemelles = @BordureColorPageFemelles, TitleFontStylePageChatons = @TitleFontStylePageChatons, TitleColorPageChatons = @TitleColorPageChatons, Info1FontStylePageChatons = @Info1FontStylePageChatons, Info1ColorPageChatons = @Info1ColorPageChatons, Info2FontStylePageChatons = @Info2FontStylePageChatons, Info2ColorPageChatons = @Info2ColorPageChatons, Info3FontStylePageChatons = @Info3FontStylePageChatons, Info3ColorPageChatons = @Info3ColorPageChatons, BordureColorPageChatons = @BordureColorPageChatons, ButtonColorPageChatons = @ButtonColorPageChatons, ButtonTextColorPageChatons = @ButtonTextColorPageChatons, ButtonTextFontStylePageChatons = @ButtonTextFontStylePageChatons, TextPageContact = @TextPageContact, TextPageCondition = @TextPageCondition, TitlePagelivreDor = @TitlePagelivreDor, TitleFontStylePagelivreDor = @TitleFontStylePagelivreDor, TitleColorPagelivreDor = @TitleColorPagelivreDor, ButtonColorPagelivreDor = @ButtonColorPagelivreDor, ButtonTextColorPagelivreDor = @ButtonTextColorPagelivreDor, ButtonTextFontStylePagelivreDor = @ButtonTextFontStylePagelivreDor, TitleFontStylePagechatProfil = @TitleFontStylePagechatProfil, TitleColorPagechatProfil = @TitleColorPagechatProfil, TextFontStylePagechatProfil = @TextFontStylePagechatProfil, TextColorPagechatProfil = @TextColorPagechatProfil, ButtonColorPagechatProfil = @ButtonColorPagechatProfil, ButtonTextColorPagechatProfil = @ButtonTextColorPagechatProfil, ButtonTextFontStylePagechatProfil = @ButtonTextFontStylePagechatProfil, BordureColorPagechatProfil = @BordureColorPagechatProfil, BagroundColorPagechatProfil = @BagroundColorPagechatProfil, TitleFontStylePagechatonProfil = @TitleFontStylePagechatonProfil, TitleColorPagechatonProfil = @TitleColorPagechatonProfil, TextFontStylePagechatonProfil = @TextFontStylePagechatonProfil, TextColorPagechatonProfil = @TextColorPagechatonProfil, StatusNameFontStylePagechatonProfil = @StatusNameFontStylePagechatonProfil, StatusNameColorPagechatonProfil = @StatusNameColorPagechatonProfil, BreedFontStylePagechatonProfil = @BreedFontStylePagechatonProfil, BreedColorPagechatonProfil = @BreedColorPagechatonProfil, BackgroundColorBreedPagechatonProfil = @BackgroundColorBreedPagechatonProfil,TextPageAccueil = @TextPageAccueil, TextFontStylePageAccueil = @TextFontStylePageAccueil, TextColorPageAccueil = @TextColorPageAccueil WHERE ProfilId = @ProfilId", connection))
                {

                     command.Parameters.AddWithValue("@ProfilId", profilId);
                    command.Parameters.AddWithValue("@BannerImages", string.Join(",", updatedBannerSection.BannerImages!));
                    command.Parameters.AddWithValue("@MaleDescription", updatedBannerSection.MaleDescription);
                    command.Parameters.AddWithValue("@KittenDescription", updatedBannerSection.KittenDescription);
                    command.Parameters.AddWithValue("@FemaleDescription", updatedBannerSection.FemaleDescription);
                    command.Parameters.AddWithValue("@TitleCard1", updatedBannerSection.TitleCard1);
                    command.Parameters.AddWithValue("@TitleCard2", updatedBannerSection.TitleCard2);
                    command.Parameters.AddWithValue("@TitleCard3", updatedBannerSection.TitleCard3);
                    command.Parameters.AddWithValue("@TitleFontStyleCard", updatedBannerSection.TitleFontStyleCard);
                    command.Parameters.AddWithValue("@TitleColorCard", updatedBannerSection.TitleColorCard);
                    command.Parameters.AddWithValue("@TextFontStyleCard", updatedBannerSection.TextFontStyleCard);
                    command.Parameters.AddWithValue("@TextColorCard", updatedBannerSection.TextColorCard);
                    command.Parameters.AddWithValue("@BackgroundColorCard", updatedBannerSection.BackgroundColorCard);
                    command.Parameters.AddWithValue("@Title", updatedBannerSection.Title);
                    command.Parameters.AddWithValue("@Subtitle", updatedBannerSection.Subtitle);
                    command.Parameters.AddWithValue("@MaleImg", updatedBannerSection.MaleImg);
                    command.Parameters.AddWithValue("@KittenImg", updatedBannerSection.KittenImg);
                    command.Parameters.AddWithValue("@FemaleImg", updatedBannerSection.FemaleImg);
                    command.Parameters.AddWithValue("@TitleFontSize", updatedBannerSection.TitleFontSize);
                    command.Parameters.AddWithValue("@TitleFontFamily", updatedBannerSection.TitleFontFamily);
                    command.Parameters.AddWithValue("@SubtitleFontSize", updatedBannerSection.SubtitleFontSize);
                    command.Parameters.AddWithValue("@SubtitleFontFamily", updatedBannerSection.SubtitleFontFamily);
                    command.Parameters.AddWithValue("@ColorHeader", updatedBannerSection.ColorHeader);
                    command.Parameters.AddWithValue("@TitleColor", updatedBannerSection.TitleColor);
                    command.Parameters.AddWithValue("@SubtitleColor", updatedBannerSection.SubtitleColor);
                    command.Parameters.AddWithValue("@Menu", updatedBannerSection.Menu != null ? string.Join(",", updatedBannerSection.Menu) : null);
                    command.Parameters.AddWithValue("@ColorMenu", updatedBannerSection.ColorMenu);
                    command.Parameters.AddWithValue("@HoverColorMenu", updatedBannerSection.HoverColorMenu);
                    command.Parameters.AddWithValue("@FontStyleMenu", updatedBannerSection.FontStyleMenu);
                    command.Parameters.AddWithValue("@ColorFooter", updatedBannerSection.ColorFooter);
                    command.Parameters.AddWithValue("@Favicon", updatedBannerSection.Favicon);
                    command.Parameters.AddWithValue("@TitlePageMales", updatedBannerSection.TitlePageMales);
                    command.Parameters.AddWithValue("@TitleFontStylePageMales", updatedBannerSection.TitleFontStylePageMales);
                    command.Parameters.AddWithValue("@TitleColorPageMales", updatedBannerSection.TitleColorPageMales);
                    command.Parameters.AddWithValue("@TextPageMales", updatedBannerSection.TextPageMales);
                    command.Parameters.AddWithValue("@TextFontStylePageMales", updatedBannerSection.TextFontStylePageMales);
                    command.Parameters.AddWithValue("@TextColorPageMales", updatedBannerSection.TextColorPageMales);
                    command.Parameters.AddWithValue("@BordureColorPageMales", updatedBannerSection.BordureColorPageMales);
                    command.Parameters.AddWithValue("@TitlePageFemelles", updatedBannerSection.TitlePageFemelles);
                    command.Parameters.AddWithValue("@TitleFontStylePageFemelles", updatedBannerSection.TitleFontStylePageFemelles);
                    command.Parameters.AddWithValue("@TitleColorPageFemelles", updatedBannerSection.TitleColorPageFemelles);
                    command.Parameters.AddWithValue("@TextPageFemelles", updatedBannerSection.TextPageFemelles);
                    command.Parameters.AddWithValue("@TextFontStylePageFemelles", updatedBannerSection.TextFontStylePageFemelles);
                    command.Parameters.AddWithValue("@TextColorPageFemelles", updatedBannerSection.TextColorPageFemelles);
                    command.Parameters.AddWithValue("@BordureColorPageFemelles", updatedBannerSection.BordureColorPageFemelles);


                    command.Parameters.AddWithValue("@TitleFontStylePageChatons", updatedBannerSection.TitleFontStylePageChatons);
                    command.Parameters.AddWithValue("@TitleColorPageChatons", updatedBannerSection.TitleColorPageChatons);
                    command.Parameters.AddWithValue("@Info1FontStylePageChatons", updatedBannerSection.Info1FontStylePageChatons);
                    command.Parameters.AddWithValue("@Info1ColorPageChatons", updatedBannerSection.Info1ColorPageChatons);
                    command.Parameters.AddWithValue("@Info2FontStylePageChatons", updatedBannerSection.Info2FontStylePageChatons);
                    command.Parameters.AddWithValue("@Info2ColorPageChatons", updatedBannerSection.Info2ColorPageChatons);
                    command.Parameters.AddWithValue("@Info3FontStylePageChatons", updatedBannerSection.Info3FontStylePageChatons);
                    command.Parameters.AddWithValue("@Info3ColorPageChatons", updatedBannerSection.Info3ColorPageChatons);
                    command.Parameters.AddWithValue("@BordureColorPageChatons", updatedBannerSection.BordureColorPageChatons);
                    command.Parameters.AddWithValue("@ButtonColorPageChatons", updatedBannerSection.ButtonColorPageChatons);
                    command.Parameters.AddWithValue("@ButtonTextColorPageChatons", updatedBannerSection.ButtonTextColorPageChatons);
                    command.Parameters.AddWithValue("@ButtonTextFontStylePageChatons", updatedBannerSection.ButtonTextFontStylePageChatons);
                    command.Parameters.AddWithValue("@TextPageContact", updatedBannerSection.TextPageContact);
                    command.Parameters.AddWithValue("@TextPageCondition", updatedBannerSection.TextPageCondition);

                    command.Parameters.AddWithValue("@TitlePagelivreDor", updatedBannerSection.TitlePagelivreDor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TitleFontStylePagelivreDor", updatedBannerSection.TitleFontStylePagelivreDor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TitleColorPagelivreDor", updatedBannerSection.TitleColorPagelivreDor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ButtonColorPagelivreDor", updatedBannerSection.ButtonColorPagelivreDor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ButtonTextColorPagelivreDor", updatedBannerSection.ButtonTextColorPagelivreDor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ButtonTextFontStylePagelivreDor", updatedBannerSection.ButtonTextFontStylePagelivreDor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TitleFontStylePagechatProfil", updatedBannerSection.TitleFontStylePagechatProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TitleColorPagechatProfil", updatedBannerSection.TitleColorPagechatProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TextFontStylePagechatProfil", updatedBannerSection.TextFontStylePagechatProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TextColorPagechatProfil", updatedBannerSection.TextColorPagechatProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ButtonColorPagechatProfil", updatedBannerSection.ButtonColorPagechatProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ButtonTextColorPagechatProfil", updatedBannerSection.ButtonTextColorPagechatProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ButtonTextFontStylePagechatProfil", updatedBannerSection.ButtonTextFontStylePagechatProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BordureColorPagechatProfil", updatedBannerSection.BordureColorPagechatProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BagroundColorPagechatProfil", updatedBannerSection.BagroundColorPagechatProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TitleFontStylePagechatonProfil", updatedBannerSection.TitleFontStylePagechatonProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TitleColorPagechatonProfil", updatedBannerSection.TitleColorPagechatonProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TextFontStylePagechatonProfil", updatedBannerSection.TextFontStylePagechatonProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TextColorPagechatonProfil", updatedBannerSection.TextColorPagechatonProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@StatusNameFontStylePagechatonProfil", updatedBannerSection.StatusNameFontStylePagechatonProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@StatusNameColorPagechatonProfil", updatedBannerSection.StatusNameColorPagechatonProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BreedFontStylePagechatonProfil", updatedBannerSection.BreedFontStylePagechatonProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BreedColorPagechatonProfil", updatedBannerSection.BreedColorPagechatonProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BackgroundColorBreedPagechatonProfil", updatedBannerSection.BackgroundColorBreedPagechatonProfil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TextPageAccueil", updatedBannerSection.TextPageAccueil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TextFontStylePageAccueil", updatedBannerSection.TextFontStylePageAccueil ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TextColorPageAccueil", updatedBannerSection.TextColorPageAccueil ?? (object)DBNull.Value);





                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok(updatedBannerSection);
                    }
                    else
                    {
                        return NotFound("Banner section not found.");
                    }
                }
            }
        }


        /* 
                    [HttpDelete("deleteMissingImages")]
                    public async Task<IActionResult> DeleteImages([FromQuery] List<string> imagePaths, [FromQuery] string directory)
                    {
                        try
                        {
                            if (imagePaths == null || imagePaths.Count == 0)
                            {
                                return BadRequest("No image paths provided.");
                            }

                            // Utiliser la variable 'directory' pour personnaliser le chemin du dossier
                            string customFolderPath = Path.Combine(_environment.WebRootPath, $"assets/{directory}");

                            foreach (var imagePath in imagePaths)
                            {
                                string fullPath = Path.Combine(customFolderPath, imagePath);
                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                            }

                            return Ok("Images deleted successfully.");
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                        }
                    }


                } */



        [Authorize]
        [HttpDelete("deleteMissingImages")]
        public async Task<IActionResult> DeleteImages([FromQuery] List<string> imagePaths, [FromQuery] string directory)
        {
            try
            {
                if (imagePaths == null || imagePaths.Count == 0)
                {
                    return BadRequest("No image paths provided.");
                }

                // Get a reference to the container
                var containerName = _configuration["AzureStorage:ContainerName"];
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                foreach (var imagePath in imagePaths)
                {
                    // Form the blob name based on the directory and image path
                    string blobName = $"{directory}/{imagePath}";

                    // Get a reference to the blob
                    var blobClient = containerClient.GetBlobClient(blobName);

                    // Check if the blob exists
                    if (await blobClient.ExistsAsync())
                    {
                        // Delete the blob
                        await blobClient.DeleteIfExistsAsync();
                    }
                }

                return Ok("Images deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
