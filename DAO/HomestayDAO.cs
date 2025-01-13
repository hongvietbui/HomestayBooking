using EXE202.Models;
using System.Data.Entity;

namespace EXE202.DAO
{
    public class HomestayDAO
    {
        public readonly EXE202Context _context;
        public HomestayDAO()
        {
        }
        public HomestayDAO(EXE202Context context)
        {
            _context = context;
        }
        public List<Homestay> GetAllHomestay()
        {
            var homestays = new List<Homestay>();
            try
            {
                homestays = _context.Homestays.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching homestay: {ex.Message}");
            }
            return homestays;
        }

        public List<Models.Host> GetAllHost()
        {
            var homestay_hosts = new List<Models.Host>();
            try
            {
                homestay_hosts = _context.Hosts.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching host: {ex.Message}");
            }
            return homestay_hosts;
        }

        /*public void UpdateImageHomestay(int homestay_id, string image_path)
        {
            try
            {
                var homestay = _context.Homestays.SingleOrDefault(h => h.HomestayId == homestay_id);
                if (homestay != null)
                {
                    homestay.Image = "/images/" + image_path;
                    _context.SaveChanges();
                }
                else
                {
                    throw new Exception("Car not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }*/
        public void AddFeedback(string feedback_content, int rating, int customer_id, int homestay_id)
        {
            try
            {
                Feedback feedback = new Feedback()
                {
                    CustomerId = customer_id,
                    HomestayId = homestay_id,
                    FeedbackContent = feedback_content,
                    Rating = rating,
                };
                _context.Feedbacks.Add(feedback);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while add feedback: {ex.Message}");
            }
        }

        public void DeleteFeedback(int feedback_id)
        {
            try
            {
                var feedback = _context.Feedbacks.SingleOrDefault(f => f.FeedbackId == feedback_id);
                _context.Feedbacks.Remove(feedback);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while delete feedback: {ex.Message}");
            }
        }

        public List<Feedback> GetAllFeedback(int homestay_id)
        {
            var feedbacks = new List<Feedback>();
            try
            {
                feedbacks = _context.Feedbacks.Where(f => f.HomestayId == homestay_id).ToList();
                return feedbacks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching feedback: {ex.Message}");
                return null;
            }
        }

        public Homestay getHomestayByID(int homestay_id)
        {
            try
            {
                var homestays = _context.Homestays.SingleOrDefault(h => h.HomestayId == homestay_id);
                return homestays;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public Models.Host GetHostByID(int homestay_id)
        {
            var homestays = _context.Homestays.SingleOrDefault(h => h.HomestayId == homestay_id);
            try
            {
                var hosts = _context.Hosts.SingleOrDefault(h => h.HostId == homestays.HostId);
                return hosts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public List<Homestay> GetRelatedHomestay(int currentHomestay_id)
        {
            var homestays = new List<Homestay>();
            try
            {
                var current_homestay = _context.Homestays.SingleOrDefault(h => h.HomestayId == currentHomestay_id);

                if (current_homestay != null)
                {
                    homestays = _context.Homestays
                        .Where(h => h.HomestayId != currentHomestay_id) // Loại bỏ xe hiện tại khỏi danh sách
                        .OrderBy(h => Math.Abs(h.PricePerNight - current_homestay.PricePerNight))
                        .Take(3)
                        .ToList();
                }
                else
                {
                    Console.WriteLine("The car with the specified ID was not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching homestay: {ex.Message}");
            }
            return homestays;
        }
        public void UpdateHomestayStatus(int homestay_id)
        {
            try
            {
                var homestay = _context.Homestays.SingleOrDefault(h => h.HomestayId == homestay_id);
                if (homestay != null)
                {
                    homestay.Status = false;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching homestay: {ex.Message}");
            }
        }

        public void UpdateHomestayStatus1(int homestay_id)
        {
            try
            {
                var homestay = _context.Homestays.SingleOrDefault(h => h.HomestayId == homestay_id);
                if (homestay != null)
                {
                    homestay.Status = true;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching homestays: {ex.Message}");
            }
        }

        public BookingContract GetBookingContractByHomestayID(int homestay_id)
        {
            try
            {
                var booking_contract = _context.BookingContracts.SingleOrDefault(b => b.RoomId == homestay_id && b.Status != "Completed" && b.Status != "Canceled");

                if (booking_contract == null)
                {
                    Console.WriteLine($"No booking contract found for ID: {homestay_id}");
                }

                return booking_contract;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching the booking contract for ID: {homestay_id}. Error: {ex.Message}");
                return null;
            }
        }

        public List<Homestay> GetHomestaysByBookingContractRoomID(int roomId)
        {
            try
            {
                return _context.Homestays.Where(h => h.HomestayId == roomId).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public bool CheckBookingContractCompleted(int homestay_id, int customerId)
        {
            try
            {
                // Kiểm tra xem có bất kỳ hợp đồng thuê nào với car_id, customerId và trạng thái là "Completed"
                bool exists = _context.BookingContracts
                    .Any(b => b.RoomId == homestay_id && b.CustomerId == customerId && b.Status == "Completed");

                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while checking the rental contract for ID: {homestay_id} and customer ID: {customerId}. Error: {ex.Message}");
                return false;
            }
        }

        public List<Homestay> FilterHomestay(string host, string homestayName, decimal? minRentalPrice, decimal? maxRentalPrice, int? max_guests, string description, string is_available)
        {
            bool is_avail = false;
            if (is_available != null)
            {
                if (is_available.Equals("true"))
                {
                    is_avail = true;
                }
            }
            var query = _context.Homestays.AsQueryable();
            if (!string.IsNullOrEmpty(host))
            {
                query = query.Where(homestay => homestay.Host.LastName.Equals(host) || homestay.Host.FirstName.Equals(host));
            }

            if (!string.IsNullOrEmpty(homestayName))
            {
                query = query.Where(homestay => homestay.Name.Contains(homestayName));
            }

            if (minRentalPrice.HasValue)
            {
                query = query.Where(homestay => homestay.PricePerNight >= minRentalPrice.Value);
            }

            if (maxRentalPrice.HasValue)
            {
                query = query.Where(homestay => homestay.PricePerNight <= maxRentalPrice.Value);
            }

            if (max_guests.HasValue)
            {
                query = query.Where(homestay => homestay.MaxGuests == max_guests.Value);
            }

            if (!string.IsNullOrEmpty(description))
            {
                query = query.Where(homestay => homestay.Description.Contains(description));
            }

            if (!string.IsNullOrEmpty(is_available))
            {
                query = query.Where(homestay => homestay.Status == is_avail);
            }

            return query.ToList();
        }

        public List<ImageHomestay> GetImageHomestaysByHomestayId(int homestayId)
        {
            var images = new List<ImageHomestay>();
            try
            {
                images = _context.ImageHomestays.Where(i => i.HomestayId == homestayId).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching homestay image: {ex.Message}");
            }
            return images;
        } 
    }
}
