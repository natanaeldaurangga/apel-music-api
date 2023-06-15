using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.DTOs.Courses;
using ApelMusic.Services;

namespace ApelMusic.Database.Seeds
{
    public class CourseSeeder
    {
        private readonly CategoryService _categoryService;

        private readonly string dummyDescription = @"
            Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis semper tortor eu justo rhoncus imperdiet. Morbi porta in libero at finibus. Aenean tellus tortor, ultrices ut justo sit amet, sollicitudin lobortis elit. Quisque finibus, odio a vulputate porttitor, elit nulla varius metus, vel tempus diam quam eget tellus. Ut tempus iaculis odio quis elementum. Fusce eget orci eget eros aliquet interdum.

            Nunc non ipsum fermentum, facilisis mi vel, pellentesque eros. Suspendisse potenti. Duis ullamcorper tortor sit amet augue rutrum sodales. Morbi nec facilisis sapien. Ut sed nisi ullamcorper, blandit massa sed, porta augue. Donec vestibulum ipsum non elit tristique, in maximus elit gravida. Donec eleifend purus eget ipsum dapibus, at tempus nunc cursus. Curabitur ornare massa id lectus tincidunt venenatis. Etiam vel justo a enim vehicula vehicula sit amet at dolor. Mauris lacinia odio vitae sapien semper semper.
        ";

        public CourseSeeder(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<int> Run()
        {
            var seeds = new List<SeedCategoryRequest>()
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

            try
            {
                seeds.ForEach(async seed => await _categoryService.InsertSeedCategoryAsync(seed));
                return 1;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

    }
}