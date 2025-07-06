import { Link } from "react-router-dom";

export const Home = () => {
  return (
    <div className="space-y-12 pb-12">
      <section className="relative bg-gradient-to-r from-indigo-500 to-purple-600 text-white py-20 px-4">
        <div className="container mx-auto text-center">
          <h1 className="text-4xl md:text-5xl font-bold mb-4">
            Delicious Foods
          </h1>
          <p className="text-xl mb-8 max-w-2xl mx-auto">
            Order from your favorite restaurants with just a few taps
          </p>
          <Link
            to="/products"
            className="bg-white text-indigo-600 px-8 py-3 rounded-full font-semibold hover:bg-gray-100 transition-colors inline-block"
          >
            Browse Menu
          </Link>
        </div>
      </section>

      {/* Placeholder for Featured Categories */}
      <section className="container mx-auto px-4">
        <h2 className="text-3xl font-bold mb-8 text-center">Our Categories</h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
          {[1, 2, 3, 4].map((item) => (
            <div
              key={item}
              className="bg-white rounded-lg shadow-md overflow-hidden p-6 text-center animate-pulse"
            >
              <div className="w-16 h-16 mx-auto mb-4 bg-gray-200 rounded-full"></div>
              <div className="h-6 bg-gray-200 rounded w-3/4 mx-auto"></div>
              <div className="h-4 bg-gray-200 rounded w-1/2 mx-auto mt-2"></div>
            </div>
          ))}
        </div>
      </section>

      {/* Placeholder for Popular Items */}
      <section className="container mx-auto px-4">
        <h2 className="text-3xl font-bold mb-8 text-center">Popular Items</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {[1, 2, 3].map((item) => (
            <div
              key={item}
              className="bg-white rounded-xl shadow-md overflow-hidden"
            >
              <div className="h-48 bg-gray-200 animate-pulse"></div>
              <div className="p-6 space-y-3">
                <div className="h-6 bg-gray-200 rounded w-3/4"></div>
                <div className="h-4 bg-gray-200 rounded w-full"></div>
                <div className="h-4 bg-gray-200 rounded w-1/2"></div>
                <div className="h-10 bg-gray-200 rounded w-full mt-4"></div>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* App Features Section */}
      <section className="container mx-auto px-4 py-12 bg-gray-50 rounded-lg">
        <h2 className="text-3xl font-bold mb-8 text-center">How It Works</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          <div className="text-center p-6">
            <div className="w-16 h-16 bg-indigo-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl">🍔</span>
            </div>
            <h3 className="font-semibold text-lg mb-2">Choose Your Food</h3>
            <p className="text-gray-600">
              Browse hundreds of delicious options
            </p>
          </div>
          <div className="text-center p-6">
            <div className="w-16 h-16 bg-indigo-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl">🚚</span>
            </div>
            <h3 className="font-semibold text-lg mb-2">Fast Delivery</h3>
            <p className="text-gray-600">Get your food delivered in minutes</p>
          </div>
          <div className="text-center p-6">
            <div className="w-16 h-16 bg-indigo-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl">💳</span>
            </div>
            <h3 className="font-semibold text-lg mb-2">Easy Payment</h3>
            <p className="text-gray-600">Secure checkout</p>
          </div>
        </div>
      </section>
    </div>
  );
};
