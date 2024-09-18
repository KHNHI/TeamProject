namespace Quanlychitieu
{
    internal class User
    {
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public int Gender { get; set; } // 1: Nam, 2: Nữ 
        public string Email { get; set; }

        public bool IsCreated { get; set; }
        //Constructor để khởi tạo đối tượng User
        public User(string name, DateTime birthdate, int gender, string email)
        {
            IsCreated = false;
            Name = name;
            BirthDate = birthdate;
            Gender = gender;
            Email = email;

           
        }

        public void CreateUser()
        {
           
            if (IsCreated)
            {
                Console.WriteLine("Thông tin người dùng đã được tạo. Bỏ qua việc tạo mới.");
                return;
            }

            Console.Write("Nhập họ và tên: ");
            Name = Console.ReadLine();

            Console.Write("Nhập giới tính (1: Nam, 2: Nữ): ");
            if (!int.TryParse(Console.ReadLine(), out int Gender) || (Gender != 1 && Gender != 2))
            {
                Console.WriteLine("Giới tính không hợp lệ. Vui lòng chọn 1 hoặc 2.");
                return;
            }

            Console.Write("Nhập ngày sinh (dd/mm/yyyy): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime birthDate))
            {
                Console.WriteLine("Ngày sinh không hợp lệ. Vui lòng nhập theo định dạng dd/mm/yyyy.");
                return;
            }
            BirthDate = birthDate;

            Console.Write("Nhập email (vd: ten@gmail.com): ");
            Email = Console.ReadLine();
            if (!Email.Contains("@gmail.com") || !Email.Contains("."))
            {
                Console.WriteLine("Email không hợp lệ. Vui lòng nhập email theo định dạng đúng.");
                return;
            }

            IsCreated = true;
            Console.WriteLine("Thông tin người dùng đã được tạo thành công.");

        }

        public void DisplayInfo()
        {
            if (!IsCreated)
            {
                Console.WriteLine("Thông tin người dùng chưa được tạo.");
                return;
            }

            Console.WriteLine("=== Thông tin cá nhân ===");
            Console.WriteLine($"Họ và tên: {Name}");
            Console.WriteLine($"Giới tính: {(Gender == 1 ? "Nam" : "Nữ")}");
            Console.WriteLine($"Ngày sinh: {BirthDate:dd/MM/yyyy}");
            Console.WriteLine($"Email: {Email}");
            Console.WriteLine("==========================");
        }
    }


}


