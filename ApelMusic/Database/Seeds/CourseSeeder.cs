using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.DTOs.Courses;
using ApelMusic.Services;
using ApelMusic.Database.Repositories;
using ApelMusic.Entities;

namespace ApelMusic.Database.Seeds
{
    public class CourseSeeder
    {
        private readonly CategoryService _categoryService;

        private readonly CourseService _courseService;

        private readonly PaymentMethodRepository _paymentRepo;

        private readonly ILogger<CourseSeeder> _logger;

        private readonly List<DateTime> dummySchedules = new()
        {
            DateTime.ParseExact("2023-07-08 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateTime.ParseExact("2023-07-10 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateTime.ParseExact("2023-07-12 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateTime.ParseExact("2023-07-15 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateTime.ParseExact("2023-07-19 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateTime.ParseExact("2023-07-23 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        };

        private readonly string dummyDescription = @"
            Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis semper tortor eu justo rhoncus imperdiet. Morbi porta in libero at finibus. Aenean tellus tortor, ultrices ut justo sit amet, sollicitudin lobortis elit. Quisque finibus, odio a vulputate porttitor, elit nulla varius metus, vel tempus diam quam eget tellus. Ut tempus iaculis odio quis elementum. Fusce eget orci eget eros aliquet interdum.

            Nunc non ipsum fermentum, facilisis mi vel, pellentesque eros. Suspendisse potenti. Duis ullamcorper tortor sit amet augue rutrum sodales. Morbi nec facilisis sapien. Ut sed nisi ullamcorper, blandit massa sed, porta augue. Donec vestibulum ipsum non elit tristique, in maximus elit gravida. Donec eleifend purus eget ipsum dapibus, at tempus nunc cursus. Curabitur ornare massa id lectus tincidunt venenatis. Etiam vel justo a enim vehicula vehicula sit amet at dolor. Mauris lacinia odio vitae sapien semper semper.
        ";

        public CourseSeeder(CategoryService categoryService, CourseService courseService, ILogger<CourseSeeder> logger, PaymentMethodRepository paymentRepo)
        {
            _categoryService = categoryService;
            _courseService = courseService;
            _paymentRepo = paymentRepo;
            _logger = logger;
        }

        public async Task<int> Run()
        {
            var categories = new List<SeedCategoryRequest>()
            {
                new SeedCategoryRequest(){ Id = Guid.NewGuid(),TagName = "Drum", Name = "Drummer Class", Image = "Constant%5C%5CDrumCategory.png", BannerImage = "Constant%5C%5CDrumCategoryBanner.png", CategoryDescription = dummyDescription },
                new SeedCategoryRequest(){ Id = Guid.NewGuid(),TagName = "Piano", Name = "Pianist Class", Image = "Constant%5C%5CPianoCategory.png", BannerImage = "Constant%5C%5CPianoCategoryBanner.jpg", CategoryDescription = dummyDescription },
                new SeedCategoryRequest(){ Id = Guid.NewGuid(),TagName = "Gitar", Name = "Gitaris Class", Image = "Constant%5C%5CGitarCategory.png", BannerImage = "Constant%5C%5CDrumCategoryBanner.png", CategoryDescription = dummyDescription },
                new SeedCategoryRequest(){ Id = Guid.NewGuid(),TagName = "Bass", Name = "Bassist Class", Image = "Constant%5C%5CGitarCategory.png", BannerImage = "Constant%5C%5CDrumCategoryBanner.png", CategoryDescription = dummyDescription },
                new SeedCategoryRequest(){ Id = Guid.NewGuid(),TagName = "Biola", Name = "Biola Class", Image = "Constant%5C%5CBiolaCategory.png", BannerImage = "Constant%5C%5CBiolaCategoryBanner.jpg", CategoryDescription = dummyDescription },
                new SeedCategoryRequest(){ Id = Guid.NewGuid(),TagName = "Menyanyi", Name = "Vocalis Class", Image = "Constant%5C%5CVocalCategory.png", BannerImage = "Constant%5C%5CDrumCategoryBanner.png", CategoryDescription = dummyDescription },
                new SeedCategoryRequest(){ Id = Guid.NewGuid(),TagName = "Flute", Name = "Flute Class", Image = "Constant%5C%5CSaxophoneCategory.png", BannerImage = "Constant%5C%5CDrumCategoryBanner.png", CategoryDescription = dummyDescription },
                new SeedCategoryRequest(){ Id = Guid.NewGuid(),TagName = "Saxophone", Name = "Saxophone Class", Image = "Constant%5C%5CSaxophoneCategory.png", BannerImage = "Constant%5C%5CDrumCategoryBanner.png", CategoryDescription = dummyDescription },
            };

            var courses = new List<SeedCourseRequest>()
            {
                new SeedCourseRequest(){ Id = Guid.NewGuid(), Name = "Kursus Drummer Special Coach (Eno Netral)", CategoryId = categories[0].Id, Image = "Constant%5C%5CCourse1.png", Description = dummyDescription, Price = 8_500_000, Schedules = dummySchedules },
                new SeedCourseRequest(){ Id = Guid.NewGuid(), Name = "[Beginner] Guitar class for kids", CategoryId = categories[2].Id, Image = "Constant%5C%5CCourse2.png", Description = dummyDescription, Price = 1_600_000, Schedules = dummySchedules },
                new SeedCourseRequest(){ Id = Guid.NewGuid(), Name = "Biola Mid-Level Course", CategoryId = categories[4].Id, Image = "Constant%5C%5CCourse3.png", Description = dummyDescription, Price = 3_000_000, Schedules = dummySchedules },
                new SeedCourseRequest(){ Id = Guid.NewGuid(), Name = "Drummer for Kids (Level Basics/1)", CategoryId = categories[0].Id, Image = "Constant%5C%5CCourse4.png", Description = dummyDescription, Price = 2_200_000, Schedules = dummySchedules },
                new SeedCourseRequest(){ Id = Guid.NewGuid(), Name = "Kursus Piano: From Zero to Pro (Full Package)", CategoryId = categories[1].Id, Image = "Constant%5C%5CCourse5.png", Description = dummyDescription, Price = 11_650_000, Schedules = dummySchedules },
                new SeedCourseRequest(){ Id = Guid.NewGuid(), Name = "Expert Level Saxophone", CategoryId = categories[7].Id, Image = "Constant%5C%5CCourse6.png", Description = dummyDescription, Price = 7_350_000, Schedules = dummySchedules },
                new SeedCourseRequest(){ Id = Guid.NewGuid(), Name = "Expert Level Drummer Lessons", CategoryId = categories[0].Id, Image = "Constant%5C%5CCourse7.png", Description = dummyDescription, Price = 5_450_000, Schedules = dummySchedules },
                new SeedCourseRequest(){ Id = Guid.NewGuid(), Name = "From Zero to Professional Drummer (Complit Package)", CategoryId = categories[0].Id, Image = "Constant%5C%5CCourse8.png", Description = dummyDescription, Price = 13_000_000, Schedules = dummySchedules },
            };

            var payments = new List<PaymentMethod>()
            {
                new PaymentMethod() { Id = Guid.NewGuid(), Image = "Constant%5C%5Cpayment-gopay.png", Name = "Gopay", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new PaymentMethod() { Id = Guid.NewGuid(), Image = "Constant%5C%5Cpayment-ovo.png", Name = "OVO", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new PaymentMethod() { Id = Guid.NewGuid(), Image = "Constant%5C%5Cpayment-dana.png", Name = "DANA", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new PaymentMethod() { Id = Guid.NewGuid(), Image = "Constant%5C%5Cpayment-mandiri.png", Name = "Mandiri", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new PaymentMethod() { Id = Guid.NewGuid(), Image = "Constant%5C%5Cpayment-bca.png", Name = "BCA", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new PaymentMethod() { Id = Guid.NewGuid(), Image = "Constant%5C%5Cpayment-bni.png", Name = "BNI", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            };

            try
            {
                categories.ForEach(async seed => await _categoryService.InsertSeedCategoryAsync(seed));
                courses.ForEach(async seed => await _courseService.InsertSeedCourseAsync(seed));
                payments.ForEach(async seed => await _paymentRepo.InsertPaymentAsync(seed));
                return 1;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}