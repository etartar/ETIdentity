using ETIdentity.CustomValidations;
using ETIdentity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ETIdentity
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("PostgreSQL"));
            });

            #region [Cookie Settings]
            CookieBuilder cookieBuilder = new CookieBuilder();
            cookieBuilder.Name = "MyBlog";
            /// Kötü niyetli kullanýcýlar client-side da benim cookie me eriþemesin diye true yapýyoruz.
            cookieBuilder.HttpOnly = true;
            /// Ne kadar süre kullanýcýnýn bilgisayarýn da kalacaðýný belirtiyoruz.
            cookieBuilder.Expiration = System.TimeSpan.FromDays(60);
            /// Ben bir cookie kaydettikten sonra sadece o site üzerinden bu cookie ye eriþebiliyorum. 
            /// Lax dersem bu özelliði kapatmýþ olurum. Strict dersem bu özelliði kýsmýþ olurum.
            /// Strict dediðim zaman sadece benim sitem üzerinden gelen cookieleri almýþ olurum.
            /// Cross-Site Request Forgery(CSRF) siteler arasý istek hýrsýzlýðý. Bu cookie nin benim sitem üzerinden gelmesi ne anlama geliyor.
            /// SameSiteMode özelliðini Strict olarak ayarlarsam istek hýrsýzlýðýnýn önüne geçmiþ olurum. Baþka bir site üzerinden browserýn cookie göndermesini engelliyorum.
            /// Farklý alt alan adlarýnýn ayný cookieyi kullanabilmeleri için bu özellik default olarak Lax ta býrakýlýr.
            cookieBuilder.SameSite = SameSiteMode.Lax;
            /// Güvenlik ayarý ile ilgili bir ayar. Eðer kullanýcý login olduðu zaman cookie oluþtuðu zaman https isteði üzerinden bu cookienin gönderilmesi.
            /// Siz bunu Always yaparsanýz browser sizin cookienizi sadece https üzerinden bir istek gelmiþ ise gönderiyor.
            /// Siz bunu SameAsRequest yaparsanýz, eðer istek http üzerinden geldiyse http, https üzerinden geldiyse https üzerinden senin cookieni gönderiyor.
            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;

            services.ConfigureApplicationCookie((options) =>
            {
                options.LoginPath = PathString.FromUriComponent("/Home/Login");
                options.LogoutPath = PathString.FromUriComponent("/Home/Logout");
                options.Cookie = cookieBuilder;
                options.SlidingExpiration = false; // Bunu true yaparsan kullanýcý benim siteme 32 gün sonra tekrar istek yaparsa expiration süresi bir 60 gün daha ötelenecek.
            });
            #endregion

            #region [Identity Settings]
            services.AddIdentity<AppUser, AppRole>((options) =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;

                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcçdefgðhýijklmnoöpqrsþtuüvwxyzABCÇDEFGÐHIÝJKLMNOPQRSÞTUÜVWXYZ0123456789-._";
            })
                .AddUserValidator<CustomUserNameValidator>()
                .AddPasswordValidator<CustomPasswordValidator>()
                .AddErrorDescriber<CustomIdentityErrorDescriber>()
                .AddEntityFrameworkStores<AppIdentityDbContext>();

            services.AddControllersWithViews();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
