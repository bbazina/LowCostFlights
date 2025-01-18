import React, { useState } from "react";
import axios from "axios";
import Icon from "@mdi/react";
import { mdiAirplaneSearch } from "@mdi/js";
import Filter from "./components/Filter";
import FlightTable from "./components/FlightTable";
import Pagination from "./components/Pagination";
import { currencyMap } from "./utils/currency";

const App = () => {
  const [flights, setFlights] = useState([]);
  const [loading, setLoading] = useState(false);

  const [pagination, setPagination] = useState({
    totalCount: 0,
    pageCount: 0,
    currentPage: 1,
    pageSize: 10,
  });

  const [filterData, setFilterData] = useState({
    originIataCode: "",
    destinationIataCode: "",
    departureDate: "",
    returnDate: "",
    numberOfPassengers: 1,
    currency: 0,
  });

  const fetchFlights = async (data) => {
    try {
      setLoading(true);
      const response = await axios.get(
        `${process.env.REACT_APP_API_BASE_URL}/api/Flight`,
        {
          params: data,
        }
      );

      const paginatedResponse = response.data;
      const transformedFlights = paginatedResponse.items.map((flight) => ({
        ...flight,
        currency: currencyMap[flight.currency] || "Unknown", // Map the currency based on the value returned
      }));
      setFlights(transformedFlights);
      setPagination((prev) => ({
        ...prev,
        totalCount: paginatedResponse.totalCount,
        pageCount: paginatedResponse.pageCount,
      }));
    } catch (error) {
      console.error("Error fetching flights:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (data) => {
    setFilterData(data);
    setPagination((prev) => ({ ...prev, currentPage: 1 })); // Reset to page 1
    fetchFlights({ ...data, page: 1, pageSize: pagination.pageSize });
  };

  const handlePageChange = (newPage) => {
    setPagination((prev) => ({ ...prev, currentPage: newPage }));
    fetchFlights({
      ...filterData,
      page: newPage,
      pageSize: pagination.pageSize,
    });
  };
  return (
    <div>
      <header>
        <h1>
          <Icon
            path={mdiAirplaneSearch}
            size={1.5}
            style={{ marginRight: "10px" }}
          />
          Low-Cost Flights Search
        </h1>
        <p>Find the best deals on your next flight!</p>
      </header>
      <Filter onSearch={handleSearch} />
      <FlightTable flights={flights} loading={loading} />
      <Pagination
        currentPage={pagination.currentPage}
        pageCount={pagination.pageCount}
        onPageChange={handlePageChange}
      />
    </div>
  );
};

export default App;
