import {Routes, Route} from "react-router";

import {UserSession} from "@features/user-profile/components/user-session";
import {Layout} from "@shared/components/layout";
import {Home} from "@features/home/components/home";
import {ProductList} from "@features/products/components/product-list.tsx";
import {ProductDetails} from "@features/products/components/product-details.tsx";
import {NewProductForm} from "@features/products/components/new-product.tsx";
import {EditProductForm} from "@features/products/components/edit-product.tsx";

function App() {
    return (
        <Routes>
            <Route path="/" element={<Layout/>}>
                <Route index element={<Home/>}/>
                <Route path="user-session" element={<UserSession/>}/>
                <Route path="products" element={<ProductList/>}/>
                <Route path="products/details/:id" element={<ProductDetails/>}/>
                <Route path="products/new" element={<NewProductForm/>}/>
                <Route path="products/edit/:id" element={<EditProductForm/>}/>
            </Route>
        </Routes>
    );
}

export default App;
