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
            /// K�t� niyetli kullan�c�lar client-side da benim cookie me eri�emesin diye true yap�yoruz.
            cookieBuilder.HttpOnly = true;
            /// Ne kadar s�re kullan�c�n�n bilgisayar�n da kalaca��n� belirtiyoruz.
            cookieBuilder.Expiration = System.TimeSpan.FromDays(60);
            /// Ben bir cookie kaydettikten sonra sadece o site �zerinden bu cookie ye eri�ebiliyorum. 
            /// Lax dersem bu �zelli�i kapatm�� olurum. Strict dersem bu �zelli�i k�sm�� olurum.
            /// Strict dedi�im zaman sadece benim sitem �zerinden gelen cookieleri alm�� olurum.
            /// Cross-Site Request Forgery(CSRF) siteler aras� istek h�rs�zl���. Bu cookie nin benim sitem �zerinden gelmesi ne anlama geliyor.
            /// SameSiteMode �zelli�ini Strict olarak ayarlarsam istek h�rs�zl���n�n �n�ne ge�mi� olurum. Ba�ka bir site �zerinden browser�n cookie g�ndermesini engelliyorum.
            /// Farkl� alt alan adlar�n�n ayn� cookieyi kullanabilmeleri i�in bu �zellik default olarak Lax ta b�rak�l�r.
            cookieBuilder.SameSite = SameSiteMode.Lax;
            /// G�venlik ayar� ile ilgili bir ayar. E�er kullan�c� login oldu�u zaman cookie olu�tu�u zaman https iste�i �zerinden bu cookienin g�nderilmesi.
            /// Siz bunu Always yaparsan�z browser sizin cookienizi sadece https �zerinden bir istek gelmi� ise g�nderiyor.
            /// Siz bunu SameAsRequest yaparsan�z, e�er istek http �zerinden geldiyse http, https �zerinden geldiyse https �zerinden senin cookieni g�nderiyor.
            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;

            services.ConfigureApplicationCookie((options) =>
            {
                options.LoginPath = PathString.FromUriComponent("/Home/Login");
                options.LogoutPath = PathString.FromUriComponent("/Home/Logout");
                options.Cookie = cookieBuilder;
                options.SlidingExpiration = false; // Bunu true yaparsan kullan�c� benim siteme 32 g�n sonra tekrar istek yaparsa expiration s�resi bir 60 g�n daha �telenecek.
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
                options.User.AllowedUserNameCharacters = "abc�defg�h�ijklmno�pqrs�tu�vwxyzABC�DEFG�HI�JKLMNOPQRS�TU�VWXYZ0123456789-._";
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
