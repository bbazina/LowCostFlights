import React from "react";
import "../styles/Pagination.css";

const Pagination = ({ currentPage, pageCount, onPageChange }) => {
  const isFirstPage = currentPage === 1;
  const isLastPage = currentPage === pageCount;

  return (
    <div className="pagination">
      <button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={isFirstPage}
      >
        Previous
      </button>
      <span>
        Page {currentPage} of {pageCount}
      </span>
      <button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={isLastPage}
      >
        Next
      </button>
    </div>
  );
};

export default Pagination;
